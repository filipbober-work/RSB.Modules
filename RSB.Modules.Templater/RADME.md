# Templater

Application receives contracts via Rabbit, compiles them to the specified template and sends them to
a Rabbit exchange.

## Add new template

Templates are discovered automatically. Application searches classes that implement ITemplateResponse<TemplateContractName>
for a given template, and for each class found assumes that there are also Rabbit contract and request classes. Path to library 
and templates is specified in application settings as TemplatesDllPath and TemplatesDirPath accordingly.

To add new template follow these steps:
* Create a new Class Library project and reference RSB.Templater for ITemplateRequest<> and ITemplateResponse<>
interfaces.
* Create Models and Template folders in the project.
* Add three classes to the Models folder:
	* Contract class that represents Rabbit contract.
	* Request class which will be used as RSB RPC request. This class must implement ITemplateRequest<ContractName> interface from RSB.Modules.Templater.Contracts library.
	* Response class which will be used as RSB RPC response. This class must implement ITemplateResponse<ContractName> interface from RSB.Modules.Templater.Contracts library.
* Add template to Templates folder. This file must have the same name as the contract name (but with ".cshtml" extension).
	* Ensure that template file "Copy to Output Directory" property is set to "Copy if newer".
* Combile library project.
* Open RSB.Templater settings and set TemplatesDirPath to Templates directory of the library project and TemplatesDllPath
to the compiled library file.

### Naming convention:

* Assume we have contract named UserRegisteredTemplate
```cs
public class UserRegisteredTemplate
{
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsPremiumUser { get; set; }
}
```

* Request class is prefixed with "Fill" and postfixed with "Request"
```cs
class FillUserRegisteredTemplateRequest : RSB.Modules.Templater.Contracts.ITemplateRequest<UserRegisteredTemplate>
{
    public UserRegisteredTemplate Template { get; set; }
}
```

* Response class is prefixed with "Fill" and postfixed with "Response"
```cs
class FillUserRegisteredTemplateResponse : RSB.Modules.Templater.Contracts.ITemplateResponse<UserRegisteredTemplate>
{
    public string Text { get; set; }
}
```

* Template for this contract may look like this:

@model Templates.Models.UserRegisteredTemplate

```html
<!DOCTYPE html>

<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>Greetings</title>
</head>
<body>
    <p>
        Hello @Model.Name,
    </p>
    <p>
        Thank you for signing up.
    </p>

    @if (!Model.IsPremiumUser)
    {
        <p>
            You are not a premium user. What a shame.
        </p>
    }

    <p>Best regards</p>

</body>
</html>
```

## Using templater

Communication with Templater is handled via RabbitMQ. For each template two exchanges will be created - one
for request and one for response.

To send example contract to the templater via Rabbit web interface do the following steps:
* Run Templater.
* Go to Exchanges -> FillContractNameRequest (FillUserRegisteredTemplateRequest in our example).
* Publish message
	* Routing key: match Templater settings. Default routing key is "DefaultTemplater".
	* Properties: reply_to = aaa, correlation_id = bbb.
	* Payload:
	```json
	{
      "Template": 
      {
         "Name": "Nameless One",
         "Email": "nameless@one.com",  
         "IsPremiumUser": false
      }
    }
	```
	
To read the result from Rabbit web interface do the following steps:
* Craete sample queue and name it for example TestQueue.
* Add binding from FillUserRegisteredResponse exchange to the newely created queue.
	* Routing key: match Templater settings. Default routing key is "DefaultTempalter".
	* To queue: TestQueue.
* Send request to templater.
* Go to Queues -> TestQueue and Get Message(s). Template should be there:
	```json
	{
      "text": "Template body"       
    }
	```

## Remove Razor warnings

Read [Razor Quickstart](https://antaris.github.io/RazorEngine/) and 
[Razor issue 244](https://github.com/Antaris/RazorEngine/issues/244) for details.

Current version of Razor (3.9.0) will not delete temporary files from \Users\<user>\AppData\Local\Temp. To
enable cleaning and to get rid of warnings Razor must run in a separate app domain. Simple solution is to
add following code at the beginning of the Main method:

```cs
static int Main()
{
	if (AppDomain.CurrentDomain.IsDefaultAppDomain())
	{
		AppDomainSetup adSetup = new AppDomainSetup();
		adSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
		var current = AppDomain.CurrentDomain;
		var strongNames = new StrongName[0];
		
		var domain = AppDomain.CreateDomain(
			"RazorAppDomain", null,
			current.SetupInformation, new PermissionSet(PermissionState.Unrestricted),
			strongNames);
		
		var exitCode = domain.ExecuteAssembly(Assembly.GetExecutingAssembly().Location);
		AppDomain.Unload(domain);
		return exitCode;
	}
	
	// Start TopShelf
	// ...
}
```			

This will enable Razor to clean up temporary files on exit.