using System;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;

namespace ElasticAgent.NETCore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Transactions transactions = new Transactions();
            await transactions.StartTransaction();
            await transactions.CaptureTransaction();
            await transactions.DeleteTransaction();

            Console.ReadKey();
        }

        public class Transactions
        {
            public async Task StartTransaction()
            {
                var startTransaction = Agent.Tracer.StartTransaction("StartTransaction", "Insert");

                try
                {
                    Person person = new Person();
                    await person.Insert();
                    throw new Exception("New Error");
                }
                catch (Exception e)
                {
                    startTransaction.CaptureException(e);
                }
                finally
                {
                    startTransaction.End();
                }
            }

            public async Task CaptureTransaction()
            {
                await Agent.Tracer.CaptureTransaction("CaptureTransaction", "Update", func: async () =>
                {
                    await Person.Update(1);
                });
            }

            public async Task DeleteTransaction()
            {
                await Agent.Tracer.CaptureTransaction("DeleteTransaction", "Delete", async () =>
                {
                    await Person.Delete(2);
                });
            }
        }

        // Simple simulation of CRUD.
        public class Person
        {
            public async Task Insert()
            {
                await Task.Delay(1);
                Console.WriteLine("Insert person");
            }

            public static async Task<int> Update(int personId)
            {
                return await Agent.Tracer.CurrentTransaction.CaptureSpan("Person", "Update", func: async () =>
                {
                    await Task.Delay(1);
                    Console.WriteLine($"Person {personId} updated.");
                    return personId;
                });
            }

            public static async Task Delete(int personId)
            {
                await Task.Delay(1);
                Console.WriteLine($"Person with ID {personId} was not deleted. Please check!");
                throw new Exception("Intentional exception");
            }

            public Task GetPersons()
            {
                Console.WriteLine("All persons returned");
                return Task.CompletedTask;
            }
        }
    }
}
