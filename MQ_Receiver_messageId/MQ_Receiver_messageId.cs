/*
 5 (Bąkowski, Strus) - odesłać elementy środkowe, 0, ostatni (ja występuję jako nadający)
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.WMQ;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections;

namespace MQ_Receiver_messageId
{
    public class MQ_Receiver_messageId
    {
        static void Main(string[] args)
        {
            string strReturn;
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
                Console.WriteLine("Server is waiting for a connection.");
                #endregion

                MQQueue queueInput = queueManager.AccessQueue("DEV.QUEUE.2LS", MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);
                MQQueue queueOutput = queueManager.AccessQueue("DEV.QUEUE.2LS", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);
                MQQueue queueOutput2 = queueManager.AccessQueue("DEV.QUEUE.2MB", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);
                MQQueue queueInput2 = queueManager.AccessQueue("DEV.QUEUE.2MB", MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING);


                List<Object> listObjects = new List<Object>();

                DataService.Receive(queueInput, queueOutput2);
                listObjects = DataService.WriteObjects(queueInput2);

                Console.Clear();
                Console.WriteLine("Dane odebrane:");

                listObjects.ForEach(i => Console.WriteLine("{0}. {1}", i.Index, i.Text));


            }
            catch (MQException MQexp)
            {
                if (MQexp.Message == "2033")
                {
                    Console.WriteLine("Kolejka jest pusta");
                }
                else
                    strReturn = "MQ Exception: " + MQexp.Message;
            }
            catch (Exception exp)
            {
                strReturn = "Exception: " + exp.Message;
            }

            Console.WriteLine("Wciśnij dowolny klawisz aby zakończyć...");
            Console.ReadKey();

        }
    }

    public class Object
    {
        public uint Index { get; private set; }
        public string Text { get; private set; }

        public Object(uint index, string text)
        {
            Index = index;
            Text = text;
        }

    }

    public class DataService
    {
        public static List<Object> WriteObjects(MQQueue queue)
        {
            List<Object> list = new List<Object>();

            byte[] spaceId = new byte[24];
            for (int i = 0; i < spaceId.Length; ++i)
                spaceId[i] = 32;

            while (true)
            {
                MQMessage queueMessage = new MQMessage();
                queueMessage.Format = MQC.MQFMT_STRING;
                MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions();
                queueGetMessageOptions.MatchOptions = MQC.MQMO_MATCH_MSG_ID;
                spaceId[0] = (byte)(1 + 32);
                queueMessage.MessageId = spaceId;
                queue.Get(queueMessage, queueGetMessageOptions);

                string message = queueMessage.ReadString(queueMessage.MessageLength);

                if (message == "END")
                    break;

                list.Add(JsonSerializer.Deserialize<Object>(message));
            }
            return list;
        }

        public static void Receive(MQQueue queue, MQQueue queue1)
        {
            while (true)
            {
                MQMessage queueMessage = new MQMessage();
                queueMessage.Format = MQC.MQFMT_STRING;
                MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions();
                MQPutMessageOptions queuePutMessageOptions = new MQPutMessageOptions();
                queueGetMessageOptions.Options += MQC.MQGMO_WAIT;
                queueGetMessageOptions.WaitInterval = 30000;
                queue.Get(queueMessage, queueGetMessageOptions);
                string message = queueMessage.ReadString(queueMessage.MessageLength);

                if (message == "END")
                {
                    queue1.Put(queueMessage, queuePutMessageOptions);
                    break;
                }
                queue1.Put(queueMessage, queuePutMessageOptions);
            }
        }
    }
}
