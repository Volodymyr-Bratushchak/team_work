using System;
using System.Data.Common;

namespace TeamWork
{
    class Program
    {
        static void Main()
        {
            List<Worker> workers = new List<Worker>();

            while (true)
            {
                Console.WriteLine("Enter the time in minutes for the worker to draw 1 picture:");
                string input = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrEmpty(input) && workers.Count > 0)
                {
                    break;
                }

                if (decimal.TryParse(input, out decimal speed))
                {
                    Worker worker = new Worker(speed);
                    workers.Add(worker);
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid speed or 'No' to stop.");
                }
            }

            Console.WriteLine("Enter the number of images:");
            if (int.TryParse(Console.ReadLine(), out int imagesNumber))
            {
                Brigade brigade = new Brigade(workers);
                brigade.CalculateImagesProcessing(imagesNumber);

                int i = 0;
                Console.WriteLine("\n");
                foreach (Worker worker in brigade.GetWorkers())
                {
                    i++;
                    Console.WriteLine("Worker #" + i + ":");
                    Console.WriteLine("Worker speed: " + Math.Round(worker.GetCapacity(), 3) + " images per minute");
                    Console.WriteLine("Worker processed images: " + worker.GetProcessedImages());
                    Console.WriteLine("Readines of the last image in progress: " + worker.PictureReadiness);
                    Console.WriteLine("Worker processed images by approximate time: " + worker.GetProcessedImagesByApproximateTime());
                    Console.WriteLine("\n");
                }

                Console.WriteLine("The brigade processed " +
                 brigade.GetProcessedImages() +
                 " image(s) for " + Math.Round(brigade.GetTime(), 3) + " minutes.");
                Console.WriteLine("The brigade processed " + brigade.GetProcessedImagesByApproximateTime() + " image(s) for approximate time: " + Math.Round(brigade.GetApproximateTime(), 3) + " minutes.");

            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number of images.");
            }
        }

    }

    public class Worker
    {
        private decimal Capacity;
        private decimal Speed;
        private int ProcessedImages;
        private int ProcessedImagesByApproximateTime;
        public decimal PictureReadiness;

        public Worker(decimal speed)
        {
            this.Capacity = 1 / speed;
            this.Speed = speed;
        }

        public void SetProcessedImages(int processedImages)
        {
            this.ProcessedImages = processedImages;
        }

        public void SetProcessedImagesByApproximateTime(int processedImages)
        {
            this.ProcessedImagesByApproximateTime = processedImages;
        }

        public int CalculateProcessedImages(decimal time)
        {
            decimal processedImages = Math.Round(time * this.Capacity, 3);
            this.PictureReadiness = processedImages - Math.Truncate(processedImages);

            return (int)Math.Floor(processedImages);

        }

        // Getter methods
        public decimal GetCapacity()
        {
            return this.Capacity;
        }

        public decimal GetSpeed()
        {
            return this.Speed;
        }

        public decimal GetProcessedImages()
        {
            return this.ProcessedImages;
        }

        public decimal GetProcessedImagesByApproximateTime()
        {
            return this.ProcessedImagesByApproximateTime;
        }
    }

    public class Brigade
    {
        private List<Worker> Workers;
        private decimal BrigadeCapacity;
        private decimal ApproximateTime;
        private decimal Time;
        private int ProcessedImages;
        private int ProcessedImagesByBrigadeAtApproximateTime;
        private int DesiredImagesNumber;
        private List<decimal> PosibleRealTimes;
        private int UnfinishedImages;


        public Brigade(List<Worker> workers)
        {
            this.Workers = workers;
            this.BrigadeCapacity = CalculateBrigadeCapacity();
            this.PosibleRealTimes = new List<decimal>();
        }

        public void CalculateImagesProcessing(int imagesNumber)
        {
            this.DesiredImagesNumber = imagesNumber;
            this.CalculateApproximateTime();
            this.CalculateApproximateBrigadeProcessedImages(this.ApproximateTime);
            this.CalculatePosibleRealTimes();
            this.CalculateRealTime();
            this.ProcessedImages = this.CalculateBrigadeProcessedImages(this.Time); ;

        }

        protected void CalculateApproximateTime()
        {
            this.ApproximateTime = this.DesiredImagesNumber / this.BrigadeCapacity;
        }

        public void CalculateApproximateBrigadeProcessedImages(decimal time)
        {
            int processedImages = 0;
            int workerProcessedImages;
            foreach (Worker worker in this.Workers)
            {
                workerProcessedImages = worker.CalculateProcessedImages(time);
                worker.SetProcessedImagesByApproximateTime(workerProcessedImages);
                processedImages += workerProcessedImages;
            }

            this.ProcessedImagesByBrigadeAtApproximateTime = processedImages;
            this.UnfinishedImages = this.DesiredImagesNumber - processedImages;
        }

        public void CalculatePosibleRealTimes()
        {
            HashSet<decimal> posibleRealTimes = new HashSet<decimal>();
            if (this.UnfinishedImages == 0)
            {
                return;
            }

            foreach (Worker worker in this.Workers)
            {
                for (int i = 1; i <= this.UnfinishedImages; i++)
                {
                    decimal posibleRealTime = worker.GetSpeed() * (worker.GetProcessedImagesByApproximateTime() + i);
                    posibleRealTimes.Add(posibleRealTime);
                }
            }

            List<decimal> posibleRealTimesList = new List<decimal>(posibleRealTimes);
            posibleRealTimesList.Sort();
            this.PosibleRealTimes = posibleRealTimesList;
        }

        private int CalculateBrigadeProcessedImages(decimal time)
        {
            int processedImages = 0;
            int workerProcessedImages;
            foreach (Worker worker in this.Workers)
            {
                workerProcessedImages = worker.CalculateProcessedImages(time);
                processedImages += workerProcessedImages;
                worker.SetProcessedImages(workerProcessedImages);
            }

            return processedImages;
        }

        public void CalculateRealTime()
        {
            int processedImages;
            if (this.UnfinishedImages == 0)
            {
                this.Time = this.ApproximateTime;
                this.ProcessedImages = this.ProcessedImagesByBrigadeAtApproximateTime;

                return;
            }

            foreach (decimal posibleRealTime in this.PosibleRealTimes)
            {
                processedImages = CalculateBrigadeProcessedImages(posibleRealTime);
                if (processedImages >= this.DesiredImagesNumber)
                {
                    this.Time = posibleRealTime;

                    break;
                }
            }


        }

        public decimal CalculateBrigadeCapacity()
        {
            decimal brigadeCapacity = 0;
            foreach (Worker worker in this.Workers)
            {
                brigadeCapacity += worker.GetCapacity();
            }

            return brigadeCapacity;
        }


        // Getter methods
        public List<Worker> GetWorkers()
        {
            return this.Workers;
        }

        public decimal GetTime()
        {
            return this.Time;
        }

        public decimal GetApproximateTime()
        {
            return this.ApproximateTime;
        }

        public decimal GetProcessedImages()
        {
            return this.ProcessedImages;
        }

        public decimal GetProcessedImagesByApproximateTime()
        {
            return this.ProcessedImagesByBrigadeAtApproximateTime;
        }

        public int GetUnfinishedImages()
        {
            return this.UnfinishedImages;
        }
    }
}
