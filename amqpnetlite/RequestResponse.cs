using System;
using System.Text;
using Amqp;
using Amqp.Sasl;
using Amqp.Framing;
using Amqp.Types;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading.Tasks;

namespace RequestRepsonse
{
    class RequestRepsonse
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

                SenderLink sender = new SenderLink(session, "request-sender", "request.ABCFR_ABCFRALMMACC1");

                Map filters = new Map();
                filters.Add(new Symbol("apache.org:selector-filter:string"), new DescribedValue(new Symbol("apache.org:selector-filter:string"), "amqp.correlation_id='123456'"));
                ReceiverLink receiver = new ReceiverLink(session, "response-receiver", new Source() { Address = "response.ABCFR_ABCFRALMMACC1", FilterSet = filters}, null);

                Message request = new Message("Hello world!");
                request.Header = new Header();
                request.Header.Durable = true;
                request.Properties = new Properties();
                request.Properties.CorrelationId = "123456";
                request.Properties.ReplyTo = "response/response.ABCFR_ABCFRALMMACC1";
                sender.Send(request);

                Message response = receiver.Receive(60000);

                if (response != null)
                {
                    Amqp.Framing.Data payload = (Amqp.Framing.Data)response.BodySection;
                    String payloadText = Encoding.UTF8.GetString(payload.Binary);

                    Console.WriteLine("Received response message: with body {1} and with correlation ID {2}", payloadText, response.Properties.CorrelationId);
                    receiver.Accept(response);
                }
                else
                {
                    Console.WriteLine("No message received for 60 seconds");
                }

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
