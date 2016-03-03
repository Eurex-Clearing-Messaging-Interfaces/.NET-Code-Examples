/**
 * ============================================================================
 * Description : FIXML Clearing API .NET broadcast receiver example
 * Version     : $Id: BroadcastReceiver.cs 3308 2015-03-17 13:51:28Z schojak $
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
            string connectionOptions = "{ }";

            string broadcastAddress = "broadcast.ABCFR_ABCFRALMMACC1.TradeConfirmation; { node: { type: queue }, create: never, mode: consume, assert: never }";

            Connection connection = null;
            Session session;

            try
            {
                /*
                 * Step 1: Preparing the connection and session
                 */
                connection = new Connection(brokerAddr, connectionOptions);
                connection.SetOption("heartbeat", "30");
                connection.SetOption("sasl_mechanisms", "EXTERNAL");
                connection.Open();

                session = connection.CreateSession();

                Console.WriteLine("-I- Connection opened, session created");

                /*
                 * Step 2: Create callback server and implicitly start it
                 */
                // The callback server is running and executing callbacks on a separate thread.
                SessionReceiver.CallbackServer cbServer = new SessionReceiver.CallbackServer(session, new Listener());

                /*
                 * Step 3: Creating message consumer
                 */
                Receiver receiver = session.CreateReceiver(broadcastAddress);
                Console.WriteLine("-I- Receiver created ");
                receiver.Capacity = 100;
                System.Threading.Thread.Sleep(100 * 1000);   // in mS
                

                /*
                 * Step 4: Stop the callback server and close receiver
                 */
                receiver.Close();
                cbServer.Close();
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

