using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.WMQ;
using System.Collections;

namespace MQ_Receiver
{
    class MQ_Receiver
    {
        static void Main(string[] args)
        {
            try
            {
                #region Connection
                Hashtable connectionProperties = new Hashtable();
                connectionProperties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
                connectionProperties.Add(MQC.HOST_NAME_PROPERTY, "148.81.200.50");
                connectionProperties.Add(MQC.PORT_PROPERTY, "1430");
                connectionProperties.Add(MQC.CHANNEL_PROPERTY, "DEV.APP.SVRCONN");
                connectionProperties.Add(MQC.USER_ID_PROPERTY, "app");
                connectionProperties.Add(MQC.PASSWORD_PROPERTY, ".Pass0,");

                MQQueueManager queueManager = new MQQueueManager("QM1", connectionProperties);
                Console.WriteLine("Connected Successfully");
                MQQueue queueInput = queueManager.AccessQueue("DEV.QUEUE.2LS", MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);
                MQQueue queueOutput = queueManager.AccessQueue("DEV.QUEUE.2MB", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);
                #endregion

                List<string> list = new List<string>();

                DataService.receive(list, queueInput);

                Console.WriteLine("Dane odebrane:");

                list.ForEach(i => Console.Write("{0} ", i));
                Console.WriteLine(); Console.WriteLine();

                list.Reverse();


                Console.WriteLine("Dane do wysłania:");

                list.ForEach(i => Console.Write("{0} ", i));
                Console.WriteLine(); Console.WriteLine();


                DataService.send(list, queueOutput);

            }
            catch (MQException MQexp)
            {
                if (MQexp.Message == "2033")
                {
                    Console.WriteLine("Kolejka jest pusta");
                }
                else
                    Console.WriteLine("MQ Exception:" + MQexp.Message);
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception:" + exp.Message);
            }

            Console.WriteLine("Wciśnij dowolny klawisz aby zakończyć...");
            Console.ReadKey();
        }
    }

    public class DataService
    {
        public static void receive(List<string> list, MQQueue queue)
        {
            while (true)
            {
                MQMessage queueMessage = new MQMessage();
                queueMessage.Format = MQC.MQFMT_STRING;
                MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions();
                queue.Get(queueMessage, queueGetMessageOptions);
                string message = queueMessage.ReadString(queueMessage.MessageLength);

                if (message == "END")
                    break;

                list.Add(message);
            }

        }

        public static void send(List<string> list, MQQueue queue)
        {

            foreach (var a in list)
            {
                string message = a;
                if (queue != null)
                {
                    MQMessage queueMessage = new MQMessage();
                    queueMessage.WriteString(message);
                    MQPutMessageOptions queuePutMessageOptions = new MQPutMessageOptions();
                    queuePutMessageOptions.Options |= MQC.MQPMO_NEW_CORREL_ID;
                    queue.Put(queueMessage, queuePutMessageOptions);
                }
            }

        }
    }


}
