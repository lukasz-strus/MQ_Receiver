using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MQ_Receiver_messageId
{
    public static class DataService
    {
        public static void WriteObjects(List<TextObject> list, MQQueue queue, byte messageId)
        {

            byte[] spaceId = new byte[24];
            for (int i = 0; i < spaceId.Length; ++i)
                spaceId[i] = 32;
            try
            {
                while (true)
                {
                    MQMessage queueMessage = new MQMessage { Format = MQC.MQFMT_STRING };
                    MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions { MatchOptions = MQC.MQMO_MATCH_MSG_ID };

                    spaceId[0] = (byte)(messageId + 32);
                    queueMessage.MessageId = spaceId;
                    queue.Get(queueMessage, queueGetMessageOptions);

                    string message = queueMessage.ReadString(queueMessage.MessageLength);

                    if (message == "END")
                        break;

                    list.Add(JsonSerializer.Deserialize<TextObject>(message));
                }                
            }
            catch (MQException)
            {
                Console.WriteLine("Kolejka pusta dla wiadomości o podanym indeksie.");
            }
        }

        public static void Receive(MQQueue queue, MQQueue queue1)
        {
            while (true)
            {
                MQMessage queueMessage = new MQMessage { Format = MQC.MQFMT_STRING };
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
