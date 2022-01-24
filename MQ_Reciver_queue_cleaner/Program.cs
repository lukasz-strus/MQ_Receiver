using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;
using IBM.WMQ;
using System.Collections;

namespace lab2_2ms
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Hashtable connectionProperties = new Hashtable();
                connectionProperties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
                connectionProperties.Add(MQC.HOST_NAME_PROPERTY, "148.81.200.50");
                connectionProperties.Add(MQC.PORT_PROPERTY, "1430");
                connectionProperties.Add(MQC.CHANNEL_PROPERTY, "DEV.APP.SVRCONN");
                connectionProperties.Add(MQC.USER_ID_PROPERTY, "app");
                connectionProperties.Add(MQC.PASSWORD_PROPERTY, ".Pass0,");                

                MQQueueManager queueManager = new MQQueueManager("QM1", connectionProperties);
                Console.WriteLine("Connected Successfully");

                while (true)
                {

                    MQQueue queue = queueManager.AccessQueue("DEV.QUEUE.2LS", MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);
                    MQMessage queueMessage = new MQMessage();
                    queueMessage.Format = MQC.MQFMT_STRING;
                    MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions();
                    queue.Get(queueMessage, queueGetMessageOptions);
                    Console.WriteLine(queueMessage.ReadString(queueMessage.MessageLength));
                }
            }
            catch (MQException MQexp)
            {
                Console.WriteLine("MQ Exception:" + MQexp.Message);
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception:" + exp.Message);
            }
        }
    }
}