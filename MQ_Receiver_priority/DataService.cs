using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace MQ_Receiver_priority
{

    public static class DataService
    {

        public static List<TextObject> WriteObjects(MQQueue queue)
        {
            List<TextObject> list = new List<TextObject>();

            while (true)
            {

                MQMessage queueMessage = new MQMessage();
                queueMessage.Format = MQC.MQFMT_STRING;
                MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions();
                queue.Get(queueMessage, queueGetMessageOptions);
                string message = queueMessage.ReadString(queueMessage.MessageLength);

                if (message == "END")
                    break;

                list.Add(JsonSerializer.Deserialize<TextObject>(message));
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
                queueGetMessageOptions.WaitInterval = 300000;
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
