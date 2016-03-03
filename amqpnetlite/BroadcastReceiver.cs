using System;
using System.Text;
using Amqp;
using Amqp.Sasl;
using Amqp.Framing;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading.Tasks;

namespace BroadcastReceiver
{
    class BroadcastReceiver
    {
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            return false;
        }


        static async Task<int> SslConnectionTestAsync()
        {
            try
            {
                ConnectionFactory factory = new ConnectionFactory();

                String certFile = "c:\\Users\\JAkub\\Downloads\\ABCFR_ABCFRALMMACC1.crt";
                factory.SSL.RemoteCertificateValidationCallback = ValidateServerCertificate;
                factory.SSL.LocalCertificateSelectionCallback = (a, b, c, d, e) => X509Certificate.CreateFromCertFile(certFile);
                factory.SSL.ClientCertificates.Add(X509Certificate.CreateFromCertFile(certFile));

                factory.AMQP.MaxFrameSize = 64 * 1024;
                factory.AMQP.ContainerId = "fixml-client";

                factory.SASL.Profile = SaslProfile.External;

                Trace.TraceLevel = TraceLevel.Frame;
                Trace.TraceListener = (f, a) => Console.WriteLine(String.Format(f, a));

                Connection.DisableServerCertValidation = false;

                Address brokerAddress = new Address("amqps://ecag-fixml-simu1.deutsche-boerse.com:10170");
                Connection connection = await factory.CreateAsync(brokerAddress);

                Session session = new Session(connection);

                ReceiverLink receiver = new ReceiverLink(session, "broadcast-receiver", "broadcast.ABCFR_ABCFRALMMACC1.TradeConfirmation");

                while (true)
                {
                    Message msg = receiver.Receive(60000);

                    if (msg == null)
                    {
                        break;
                    }

                    Amqp.Framing.Data payload = (Amqp.Framing.Data)msg.BodySection;
                    String payloadText = Encoding.UTF8.GetString(payload.Binary);

                    Console.WriteLine("Received message: {0}", payloadText);
                    receiver.Accept(msg);
                }
                
                Console.WriteLine("No message received for 60 seconds");

                await connection.CloseAsync();

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0}.", e);
                return 1;
            }
        }

        static void Main(string[] args)
        {
            Task<int> task = SslConnectionTestAsync();
            task.Wait();
        }
    }
}
