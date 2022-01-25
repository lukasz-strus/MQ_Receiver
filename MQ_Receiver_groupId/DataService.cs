using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MQ_Receiver_groupId
{
    public static class DataService
    {
        /// <summary>
        /// Metoda wczytująca obiekty z drugiej kolejki i wypisująca je do Consoli.
        /// </summary>
        /// <param name="queue">Kolejka pomocnicza</param>
        /// <param name="numberOfMessages">Przesłana liczba elementów w kolejce</param>
        /// <returns></returns>
        public static List<TextObject> WriteObjects(MQQueue queue)
        {
            List<TextObject> list = new List<TextObject>();

            byte groupId;

            byte[] spaceId = new byte[24];
            for (int i = 0; i < spaceId.Length; ++i)
                spaceId[i] = 32;

            #region GroupId
            MQMessage groupIdMessage = new MQMessage
            {
                Format = MQC.MQFMT_STRING,
                Priority = 9
            };
            MQGetMessageOptions queueGetGroupMessageOptions = new MQGetMessageOptions();
            queue.Get(groupIdMessage, queueGetGroupMessageOptions);
            groupId = Convert.ToByte(groupIdMessage.ReadString(groupIdMessage.MessageLength));
            Console.WriteLine("GroupeID wynosi: " + groupId);
            #endregion

            #region Messages
            spaceId[0] = (byte)(groupId + 32);
            while (true)
            {
                MQMessage queueMessage = new MQMessage
                {
                    Format = MQC.MQFMT_STRING,
                    MessageFlags = MQC.MQGS_MSG_IN_GROUP
                };
                MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions
                {
                    MatchOptions = MQC.MQMO_MATCH_GROUP_ID
                };
                queueGetMessageOptions.Options |= MQC.MQGMO_ALL_MSGS_AVAILABLE;
                queueMessage.GroupId = spaceId;
                queue.Get(queueMessage, queueGetMessageOptions);
                string message = queueMessage.ReadString(queueMessage.MessageLength);

                if (message == "END")
                    break;

                list.Add(JsonSerializer.Deserialize<TextObject>(message));
            }
            #endregion

            return list;
        }


        /// <summary>
        /// Czeka na połączenie, następnie przenosi objekty z jednej kolejki do drugiej.
        /// </summary>
        /// <param name="queue">Kolejka z której będą odbierane komunikaty</param>
        /// <param name="queue1">Kolejka do której będą wysyłane komunikaty</param>
        public static void ReceiveObjects(MQQueue queue, MQQueue queue1)
        {
            try
            {
                MQMessage queueFirstMessage = new MQMessage { Format = MQC.MQFMT_STRING };
                MQPutMessageOptions queuePutMessageOptions = new MQPutMessageOptions();
                MQGetMessageOptions queueGetFirstMessageOptions = new MQGetMessageOptions();
                queueGetFirstMessageOptions.Options += MQC.MQGMO_WAIT;
                queueGetFirstMessageOptions.WaitInterval = MQC.MQWI_UNLIMITED;
                queue.Get(queueFirstMessage, queueGetFirstMessageOptions);
                queue1.Put(queueFirstMessage, queuePutMessageOptions);

                while (true)
                {
                    MQMessage queueMessage = new MQMessage { Format = MQC.MQFMT_STRING };
                    MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions();
                    queue.Get(queueMessage, queueGetMessageOptions);
                    queue1.Put(queueMessage, queuePutMessageOptions);
                }
            }
            catch (MQException) { }
        }

    }
}
