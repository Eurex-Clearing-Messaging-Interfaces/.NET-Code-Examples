/**
 * ============================================================================
 * Description : FIXML Clearing API .NET broadcast receiver example
 * Version     : $Id$
 * ============================================================================
 */

using System;
using Org.Apache.Qpid.Messaging;
using Org.Apache.Qpid.Messaging.SessionReceiver;

namespace Org.Apache.Qpid.Messaging.Examples
{
    class Listener : ISessionReceiver
    {
        // Callback method to be called when message arrives
        public void SessionReceiver(Receiver receiver, Message message)
        {
            Console.WriteLine("-I Message received with content: ", message.GetContent());
            receiver.Session.Acknowledge();
        }

        public void SessionException(Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    class BroadcastReceiver
    {
        static int Main(string[] args)
        {
            string[] loggerOptions = {"", "--log-enable", "info+", "--log-to-stdout", "on", "--log-time", "on", "--log-level", "on"};
            Logger logger = new Logger();
            logger.Configure(loggerOptions);

            string brokerAddr = "amqp:ssl:ecag-fixml-simu1.deutsche-boerse.com:10170";
            string connectionOptions = "{ protocol: amqp1.0 }";

            string replyAddress = "response/response.CBKFR_TESTCALMMACC1; { node: { type: topic }, assert: never, create: never }";
            string requestAddress = "request.CBKFR_TESTCALMMACC1; { node: { type: topic }, assert: never, create: never }";
            string responseAddress = "response.CBKFR_TESTCALMMACC1; {create: never, assert: never, node: { type: queue } }";

            Connection connection = null;
            Session session;

            try
            {
                /*
                 * Step: Preparing the connection and session
                 */
                connection = new Connection(brokerAddr, connectionOptions);
                connection.SetOption("sasl_mechanisms", "EXTERNAL");
                connection.Open();

                session = connection.CreateSession();

                Console.WriteLine("-I- Connection opened, session created");

                /*
                 * Step: Creating message consumer
                 */
                Receiver receiver = session.CreateReceiver(responseAddress);
                receiver.Capacity = 100;

                Sender sender = session.CreateSender(requestAddress);
                sender.Capacity = 100;

                Console.WriteLine("-I- Receiver and sender created ");

                /*
                 * Step: Send request message 
                 */
                Message requestMsg = new Message("<FIXML>...</FIXML>");
                requestMsg.ReplyTo = new Address(replyAddress);
                sender.Send(requestMsg);

                Console.WriteLine("-I- Request sent: {0}", requestMsg.GetContent());

                /*
                 * Step: Receive response message 
                 */
                Message msg = receiver.Fetch(DurationConstants.SECOND * 100);
                Console.WriteLine("-I- Response message received with content: {0}", msg.GetContent());

                /*
                 * Step: Stop the callback server and close receiver
                 */
                receiver.Close();
                sender.Close();
                session.Close();
            }
            catch (QpidException ex)
            {
                Console.WriteLine("-E- QpidException caught: {0}", ex.Message);
                return 1;
            }
            finally
            {
                if (connection != null && connection.IsOpen)
                {
                    Console.WriteLine("-I- Closing the connection.");
                    connection.Close();
                }
            }
            return 0;
        }
    }
}
