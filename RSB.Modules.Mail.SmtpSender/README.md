
  # SmtpSender

Application receives messages via Rabbit and them as SMTP emails.

# Send email

 To send an email ensure that there are correct credentials for email server in application settings, run
 application and send contract that fulfills SendMailMessage class requirements to SendMailMessage exchange.
 Ensure that routing key matches the one in application settings.

## Sending email from Rabbit web interface example:
* Run application.
* Go to Exchanges -> SendMailMessage
* Publish message with following parameters:
	* Routing key: Match the one in application settings. Default is "DefaultSmtp"
	* Prepare json message, for example
	```json
	{
      "FromMail": "HostAddress",
      "FromName": "Hostname",
      "Recipients":
      [
       { "ToMail": "example@mail.com",
         "ToName": "Nameless One" }
      ],
      "Subject": "Return to sender",
      "Body": "Example mail body."
    }
	```
## Sending email from code example:
* Add reference to RSB.Modules.Mail.Contracts.
* Create SendMailMessage instance.
* Fill fields.
* Send created contract via Rabbit.

```cs
var message = new SendMailMessage
    {
        FromMail = "FromMail",
        FromName = "FromName",
        Recipients = new List<Recipient>() { new Recipient { ToMail = "nameless@one.com"", ToName = "Nameless One" } },
        Body = "Sample mail body",
        Subject = "Mail subject"
    };

bus.Enqueue(message, "DefaultSmtp");
```

# SendMailMessage contract

 For the message to be processed it must be in following format:
 ```cs
namespace RSB.Modules.Mail.Contracts
{
    public class SendMailMessage
    {
        public string FromMail { get; set; }
        public string FromName { get; set; }
        public List<Recipient> Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
 ```

 Reference RSB.Modules.Mail.Contracts in you project.

 # Attaching images

 MailSender is searching every mail message body for particular expressions that indicate there are images to
 be attached. There are two supported ways to include and image, both require html body.

 ## Attach image from file

 Images may be attached from file. For this method to work, image must be marked with cid and must be available for the
 MailSender to read. Following code will tell MailSender to attach image file named img.png in img folder.

 ```html
 <img src="cid:img/img.png" alt='' data-default="placeholder" data-max-width="560" />
 ```

 ## Attach image encoded as Base64

 Another way to include images is to encode with Base64 and store in message body directly. It makes message body much
 larger but images are not required to be available to the mail sender directly. This method also requires cid to be
 specified.

 ```html
 <img alt="Sample image" src="cid:anyWordCharacters,data:image/gif;base64,R0lGODlhPQBEAPeoAJosM//AwO/AwHVYZ/z595k..." data-default="placeholder" data-max-width="560" />
 ```


