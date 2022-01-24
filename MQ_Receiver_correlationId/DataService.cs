using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MQ_Receiver_correlationId
{
    public static class DataService
    {
        /// <summary>
        /// Metoda wczytująca obiekty z drugiej kolejki i wypisująca je do Consoli.
        /// </summary>
        /// <param name="queue">Kolejka pomocnicza</param>
        /// <param name="numberOfMessages">Przesłana liczba elementów w kolejce</param>
        /// <returns></returns>
        public static List<TextObject> WriteObjects(MQQueue queue, out int numberOfMessages)
        {
            List<TextObject> list = new List<TextObject>();

            byte correlationId;

            #region CorrelationId
            MQMessage correlationIdMessage = new MQMessage { Format = MQC.MQFMT_STRING };
            MQGetMessageOptions queueGetcorrelationMessageOptions = new MQGetMessageOptions { MatchOptions = MQC.MQMO_MATCH_CORREL_ID };
            correlationIdMessage.Priority = 9;
            queue.Get(correlationIdMessage, queueGetcorrelationMessageOptions);
            correlationId = Convert.ToByte(correlationIdMessage.ReadString(correlationIdMessage.MessageLength));
            #endregion


            byte[] spaceId = new byte[24];
            for (int i = 0; i < spaceId.Length; ++i)
                spaceId[i] = 32;

            spaceId[0] = (byte)(correlationId + 32);

            QueueBrowse(queue);

            #region NumbersOfMessages
            MQMessage queueFirstMessage = new MQMessage { Format = MQC.MQFMT_STRING };
            MQGetMessageOptions queueGetFirstMessageOptions = new MQGetMessageOptions { MatchOptions = MQC.MQMO_MATCH_CORREL_ID };
            queueFirstMessage.CorrelationId = spaceId;
            queue.Get(queueFirstMessage, queueGetFirstMessageOptions);
            numberOfMessages = Convert.ToInt32(queueFirstMessage.ReadString(queueFirstMessage.MessageLength));
            #endregion

            #region Messages

            while (true)
            {
                MQMessage queueMessage = new MQMessage { Format = MQC.MQFMT_STRING };
                MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions { MatchOptions = MQC.MQMO_MATCH_CORREL_ID };
                queueMessage.CorrelationId = spaceId;
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
        /// Metoda przeszukająca kolejkę (BROWSE).
        /// </summary>
        /// <param name="queue">Kolejka do przeszukania</param>
        /// <exception cref="MQException"/>
        private static void QueueBrowse(MQQueue queue)
        {
            Console.WriteLine("BROWSE:\n");
            int numbersOfMessagesBrowse = 0;

            #region BrowsFirstMessage
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
            Console.WriteLine("Liczba nadanych komunikatów: " + firstMessage + "\n");
            #endregion

            #region BrowseNextMesseges
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

                    if (message != "END")
                    {
                        Console.WriteLine(message);
                        numbersOfMessagesBrowse++;
                    }
                }
            }
            catch (MQException)
            {
                Console.WriteLine("Wszystkie dane zostały wczytane.\n");
            }

            #endregion

            Console.WriteLine("Liczba komunikatów w kolejce: " + numbersOfMessagesBrowse + "\n");

            if (numbersOfMessagesBrowse == firstMessage)
                Console.WriteLine("Liczba komunikatów wysłanych ZGADZA się z liczbą komunikatów w kolejce.\n");
            else
                Console.WriteLine("Liczba komunikatów wysłanych NIE ZGADZA się z liczbą komunikatów w kolejce.\n");

            Console.WriteLine("Wciśnij dowolny klawisz aby kontynuować...");
            Console.ReadKey();
        }

        /// <summary>
        /// Czeka na połączenie, następnie przenosi objekty z jednej kolejki do drugiej.
        /// </summary>
        /// <param name="queue">Kolejka z której będą odbierane komunikaty</param>
        /// <param name="queue1">Kolejka do której będą wysyłane komunikaty</param>
        public static void ReceiveObjects(MQQueue queue, MQQueue queue1)
        {
            while (true)
            {
                MQMessage queueMessage = new MQMessage { Format = MQC.MQFMT_STRING };
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
