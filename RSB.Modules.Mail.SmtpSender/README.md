
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


