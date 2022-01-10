using System;
using Elastic.Apm;
using Elastic.Apm.Api;

namespace ElasticAgent.NETCore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Transactions transactions = new Transactions();
            await transactions.StartTransaction().ConfigureAwait(false);
            await transactions.CaptureTransaction().ConfigureAwait(false);
            await transactions.DeleteTransaction().ConfigureAwait(false);

            Console.ReadKey();
        }

        public class Transactions
        {
            public async Task<Task> StartTransaction()
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
                return Task.CompletedTask;
            }

            public Task<Task> CaptureTransaction()
            {
                var captureTransaction = Agent.Tracer.CaptureTransaction("CaptureTransaction", "Update", func: async ()
                    =>
                {
                    await Person.Update(1);
                });

                return Task.FromResult(Task.CompletedTask);
            }

            public Task<Task> DeleteTransaction()
            {
                Agent.Tracer.CaptureTransaction("DeleteTransaction", "Delete", async () =>
                {
                    await Person.Delete(2);
                });
                return Task.FromResult(Task.CompletedTask);
            }
        }

        // Simple simulation of CRUD.
        public class Person
        {
            public async Task<Task> Insert()
            {
                await Task.Delay(1);

                Console.WriteLine("Insert person");

                return Task.CompletedTask;
            }

            public static async Task<int> Update(int personId)
            {
                return await Agent.Tracer.CurrentTransaction.CaptureSpan("Person", "Update", func: async ()
                     =>
                {
                    await Task.Delay(1);

                    Console.WriteLine($"Person {personId} updated.");

                    return personId;
                });
            }

            public static async Task<Task> Delete(int personId)
            {
                await Task.Delay(1);

                Console.WriteLine($"Person with ID {personId} was not deleted. Please check!");

                throw new Exception("Intentional exception");
            }

            public async Task<Task> GetPersons()
            {
                await Task.Delay(1);

                Console.WriteLine("All persons returned");

                return Task.CompletedTask;
            }
        }
    }
}