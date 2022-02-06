/*
2 (Bąkowski, Strus) - odesłać elementy: 0, ostatni, pozostałe
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
namespace MQ_Receiver_Transakcje
{
    class MQ_Receiver_Transakcje
    {

        static void Main(string[] args)
        {
            string strReturn;
            try
            {
                #region Connection
                Hashtable connectionProperties = new Hashtable
                {
                    { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED },
                    { MQC.HOST_NAME_PROPERTY, "148.81.200.50" },
                    { MQC.PORT_PROPERTY, "1430" },
                    { MQC.CHANNEL_PROPERTY, "DEV.APP.SVRCONN" },
                    { MQC.USER_ID_PROPERTY, "app" },
                    { MQC.PASSWORD_PROPERTY, ".Pass0," }
                };
                MQQueueManager queueManager = new MQQueueManager("QM1", connectionProperties);
                Console.WriteLine("Server is waiting for a connection.");
                #endregion

                MQQueue queueInput = queueManager.AccessQueue("DEV.QUEUE.2LS", MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);
                MQQueue queueOutput = queueManager.AccessQueue("DEV.QUEUE.2LS", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);
                MQQueue queueOutput2 = queueManager.AccessQueue("DEV.QUEUE.2MB", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);
                MQQueue queueInput2 = queueManager.AccessQueue("DEV.QUEUE.2MB", MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING);

                List<TextObject> listObjects = new List<TextObject>();


                DataService.ReceiveObjects(queueInput, queueOutput2);

                listObjects = DataService.WriteObjects(queueInput2);
                Console.WriteLine("Dane odebrane:");
                listObjects.ForEach(i => Console.WriteLine("{0}. {1}", i.Index, i.Text));

                while (true)
                {
                    Console.WriteLine("Wprowadź \"C\" jeżeli chcesz zatwierdź transakcje.");
                    Console.WriteLine("Wprowadź \"B\" jeżeli chcesz anulować transakcje (wiadomości pozostaną dalej w kolejce).");
                    string choice = Console.ReadLine();

                    if (choice.ToUpper() == "C")
                    {
                        queueManager.Commit();
                        listObjects = DataService.WriteObjects(queueInput2);
                        Console.WriteLine("Dane odebrane:");
                        listObjects.ForEach(i => Console.WriteLine("{0}. {1}", i.Index, i.Text));
                        break;
                    }
                    else if (choice.ToUpper() == "B")
                    {
                        queueManager.Backout();
                        listObjects = DataService.WriteObjects(queueInput2);
                        Console.WriteLine("Dane odebrane:");
                        listObjects.ForEach(i => Console.WriteLine("{0}. {1}", i.Index, i.Text));                   
                    }
                    else
                        Console.WriteLine("Wybrałeś złą opcje, spróbuj wybrać jeszcze raz.");
                }
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
            Console.ReadLine();

        }
    }
}
