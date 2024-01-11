using Domain.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Application.Service
{
    public class EmailService : IEmailService
    {
        readonly string EmailDeServico;
        readonly string SenhaDoEmail;
        readonly string EnderecoSmtp;
        readonly int PortaSmtp; 
        public EmailService() {
            EmailDeServico = "ocl0ck@outlook.com";
            SenhaDoEmail = "Yk!;he!)B'N373/";
            EnderecoSmtp = "smtp.office365.com";
            PortaSmtp = 587;
        }

        

        public void EnviarEmailRegistroDispositivo(DateTime dataHoraRegistro, string emailDoUsuario)
        {
            try
            {
                var message = new MimeMessage();
                string destinatario = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" ? emailDoUsuario : "ffersants15@gmail.com";
                // Set the sender and recipient addresses
                message.From.Add(new MailboxAddress("O'Clock", EmailDeServico));
                message.To.Add(new MailboxAddress("", emailDoUsuario));

                // Set the subject and body of the email
                message.Subject = "O'Clock - Redefinir senha de acesso";
                message.Body = new TextPart("html")
                {
                    Text = @$"<!DOCTYPE html>
								<html lang='en'>
									<head>
										<link
											href='https://fonts.googleapis.com/css2?family=Poppins:wght@400;700&amp;family=Roboto:wght@400;500&amp;display=swap'
											rel='stylesheet'
										/>
										<meta charset='UTF-8' />
										<meta name='viewport' content='width=device-width, initial-scale=1.0' />
										<title>Document</title>
									</head>
									<body>
										<h1>Novo dispositivo cadastrado</h1>
									</body>
								</html>"
                };

                // Create a new SmtpClient
                using (var client = new SmtpClient())
                {
                    // Connect to the SMTP server
                    client.Connect(EnderecoSmtp, PortaSmtp, SecureSocketOptions.StartTls);

                    // Authenticate with the SMTP server if needed
                    client.Authenticate(EmailDeServico, SenhaDoEmail);

                    // Send the email
                    client.Send(message);

                    // Disconnect from the SMTP server
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during email sending
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }
    }
}