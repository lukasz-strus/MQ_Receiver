﻿using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MQ_Receiver_correlationId
{

    public static class DataService
    {
        public static List<TextObject> WriteObjects(MQQueue queue, byte correlationId, out int numberOfMessages)
        {
            List<TextObject> list = new List<TextObject>();

            #region correlationId
            byte[] spaceId = new byte[24];
            for (int i = 0; i < spaceId.Length; ++i)
                spaceId[i] = 32;

            spaceId[0] = (byte)(correlationId + 32);
            #endregion

            QueueBrowse(queue);

            #region ListLength
            MQMessage queueFirstMessage = new MQMessage { Format = MQC.MQFMT_STRING };
            MQGetMessageOptions queueGetFirstMessageOptions = new MQGetMessageOptions() { MatchOptions = MQC.MQMO_MATCH_CORREL_ID };
            queueFirstMessage.CorrelationId = spaceId;
            queue.Get(queueFirstMessage, queueGetFirstMessageOptions);
            numberOfMessages = Convert.ToInt32(queueFirstMessage.ReadString(queueFirstMessage.MessageLength));
            #endregion


            while (true)
            {
                MQMessage queueMessage = new MQMessage() { Format = MQC.MQFMT_STRING };
                MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions() { MatchOptions = MQC.MQMO_MATCH_CORREL_ID };
                queueMessage.CorrelationId = spaceId;
                queue.Get(queueMessage, queueGetMessageOptions);
                string message = queueMessage.ReadString(queueMessage.MessageLength);

                if (message == "END")
                    break;

                list.Add(JsonSerializer.Deserialize<TextObject>(message));
            }
            return list;
        }

        private static void QueueBrowse(MQQueue queue)
        {

            Console.WriteLine("BROWSE:");
            int numbersOfMessagesBrowse = 0;
            MQMessage queueMessage = new MQMessage
            {
                Format = MQC.MQFMT_STRING,
                MessageId = MQC.MQMI_NONE,
                CorrelationId = MQC.MQMI_NONE
            };

            MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions();
            queueGetMessageOptions.Options += MQC.MQGMO_WAIT + MQC.MQGMO_BROWSE_FIRST;
            queue.Get(queueMessage, queueGetMessageOptions);
            int firstMessage = Convert.ToInt32(queueMessage.ReadString(queueMessage.MessageLength));
            Console.WriteLine("Liczba nadanych komunikatów: " + firstMessage);

            try
            {
                while (true)
                {
                    MQMessage queueNextMessage = new MQMessage
                    {
                        Format = MQC.MQFMT_STRING,
                    };

                    MQGetMessageOptions queueGetNextMessageOptions = new MQGetMessageOptions();
                    queueGetNextMessageOptions.Options += MQC.MQGMO_WAIT + MQC.MQGMO_BROWSE_NEXT;
                    queue.Get(queueNextMessage, queueGetNextMessageOptions);
                    string message = queueNextMessage.ReadString(queueNextMessage.MessageLength);

                    if (message != "END" && message !="KONIEC")
                    {
                        Console.WriteLine(message);
                        numbersOfMessagesBrowse++;
                    }
                }
            }

            catch (MQException)
            {
                Console.WriteLine("Wszystkie dane zostały wczytane.");
            }

            Console.WriteLine("Liczba komunikatów w kolejce: " + numbersOfMessagesBrowse);

            if (numbersOfMessagesBrowse == firstMessage)
                Console.WriteLine("Liczba komunikatów wysłanych zgada się z liczbą komunikatów w kolejce.");
            else
                Console.WriteLine("Liczba komunikatów wysłanych NIE zgadza się z liczbą komunikatów w kolejce.");

            Console.WriteLine("Wciśnij dowolny klawisz aby kontynuować...");
            Console.ReadKey();
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
