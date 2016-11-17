# SmtpSender

Application receives messages via Rabbit and them as SMTP emails.

# Send email

 To send an email ensure that there are correct credentials for email server in application settings, run
 application and send contract that fulfills SendMailMessage class requirements to SendMailMessage exchange.
 Ensure that routing key matches the one in application settings. 
 
 # Sending email from Rabbit web interface example:
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
 # SendMailMessage contract
 
 For the message to be processed it must be in following format:
 ```cs
public class SendMailMessage
{
    public string FromMail { get; set; }
    public string FromName { get; set; }
    public List<Recipient> Recipients { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
 
 ```


