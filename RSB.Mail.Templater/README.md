# Mail Templater

Application receives messages via Rabbit, compiles them to the specified template and sends them as email to the
given recipients.

## Add new template

Email templates are discovered automatically. Application searches for classes that implement IMailMessage interface
in a DLL and compiles them using corresponding templates. Path to library and templates is specified in application
settings as TemplatesDll and TemplatesDir accordingly.

To add new templates do the following steps:
* Create a new Class Library project and reference RSB.Mail.Templater for IMailMessage interface
and MailProperties.
* Create Models and Templates folders.
* Add model class to the Models folder. This class must implement RSB.Mail.Templater.Models.IMailMessage interface.
* Add a template to Templates folder. This template must be named identically to the corresponding model class, but
with .cshtml extension.
* Ensure that template file "Copy to Output Directory" property is set to "Copy if newer".
* Compile library project.
* Open RSB.Mail.Templater settings and set TemplatesDir to Templates directory and TemplatesDll to library file.
* Run RSB.Mail.Templater.

Sample EmailTemplates library:

* Model
```cs
using RSB.Mail.Templater.Models;

namespace EmailTemplates.Models
{
    public class SendUserRegisteredMail : IMailMessage
    {
        public MailProperties Properties { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsPremiumUser { get; set; }
    }
}
```

* Template

```html
@model EmailTemplates.Models.SendUserRegisteredMail

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


## Send email

To send an email simply publish message via rabbit to the exchange named after model.

Sending SendUserRegisteredMail email to the example@mail.com recipient.

```json
{"Properties":
 {
  "FromMail": "HostAddress",
  "FromName": "Hostname",
  "Recipients":
  [
   { "ToMail": "fxonus.mail@gmail.com",
     "ToName": "Nameless One" }
  ],
  "Subject": "Return to sender",
 },
  "Name": "Nameless One",
  "Email": "nameless@one.com",
  "IsPremiumUser": false
}
```

Sender address is specified in application settings, while recipient address is contained in Rabbit message (ToMail).
After rabbit message is received, the email template is sent to the list of recipients.

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

