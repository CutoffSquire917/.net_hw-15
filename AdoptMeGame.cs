using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class AdoptMeGame
    {
        private event Action? messageHandler;

        public class Pet
        {
            public string Name { get; private init; } = "";
            public string Species { get; private init; } = "";
            private int _hungry = 0;
            public int Hungry
            {
                get => _hungry;
                set => _hungry = Math.Clamp(value, 0, 100);
            }
            private int _happiness = 0;
            public int Happiness
            {
                get => _happiness;
                set => _happiness = Math.Clamp(value, 0, 100);
            }
            private int _energy = 0;
            public int Energy
            {
                get => _energy;
                set => _energy = Math.Clamp(value, 0, 100);
            }
            public Pet(string name, string species, int hungry, int happiness, int energy)
            {
                Name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
                Species = char.ToUpper(species[0]) + species.Substring(1).ToLower();
                Hungry = hungry > 100 ? 100 : hungry;
                Happiness = happiness > 100 ? 100 : happiness;
                Energy = energy > 100 ? 100 : energy;
            }
        }
        public List<Pet> Pets { get; private set; } = new();
        private int Index { get; set; } = 0;
        private Timer _timer { get; set; }
        public string SavesPath { get; set; }

        public void CreatePet()
        {
            Console.Clear();
            Console.WriteLine("\tAdopt me\n\n");
            string name = "";
            string species = "";

            Console.WriteLine();
            while (name == "" || !(name.All(c => char.IsLetter(c) || c == '-')))
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write("Add name: ");
                name = Console.ReadLine();
            }

            Console.WriteLine();
            while (species == "" || !(species.All(c => char.IsLetter(c) || c == '-')))
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write("Specify the species: ");
                species = Console.ReadLine();
            }

            Console.WriteLine("\nAre you sure ?");
            if (ChoicePanel("Yes", "No") == 0)
            {
                Pets.Add(new Pet(name, species, 100, 50, 100));
                Index = Pets.Count - 1;
                _timer = new Timer(
                callback: Expenses,
                state: null,
                dueTime: TimeSpan.FromMinutes(1),
                period: TimeSpan.FromMinutes(1)
            );
            }
        }
        private void Expenses(object state)
        {
            var pet = Pets[Index];
            pet.Hungry -= 10;
            pet.Happiness -= 5;
            pet.Energy -= 5;

            for (int i = 3; i < 6; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
            }
            Console.SetCursorPosition(0, 3);
            Console.WriteLine($"hungry    - {pet.Hungry}" +
                $"\nhappiness - {pet.Happiness}" +
                $"\nenergy    - {pet.Energy}");
        }

        public void Feed(Pet pet)
        {
            int random_number = new Random().Next(1, 101);
            if (random_number <= 85)
            {
                pet.Hungry += 20;
            }
            else if (random_number <= 15)
            {
                pet.Hungry += 40;
            }
            else
            {
                pet.Hungry += 10;
                pet.Happiness -= 10;
            }

        }
        public void PlayWithPet(Pet pet)
        {
            int random_number = new Random().Next(1, 6);
            pet.Happiness += random_number * 5;
            pet.Energy -= 10 + (random_number / 5 * 5);
        }

        private Timer _sleepTimer;
        public void PutToSleep(Pet pet)
        {
            _timer?.Dispose();
            _timer = null;
            _timer = new Timer(
                callback: ExpensesDuringSleep,
                state: pet,
                dueTime: TimeSpan.FromSeconds(40),
                period: TimeSpan.FromSeconds(40));
            _sleepTimer = new Timer(
                callback: Sleep,
                state: pet,
                dueTime: TimeSpan.Zero,
                period: TimeSpan.FromSeconds(15));

            Console.ReadKey();

            _sleepTimer?.Dispose();
            _sleepTimer = null;
            _timer?.Dispose();
            _timer = null;
            _timer = new Timer(
                callback: Expenses,
                state: null,
                dueTime: TimeSpan.FromMinutes(1),
                period: TimeSpan.FromMinutes(1));
        }
        private void Sleep(object state)
        {
            var pet = (Pet)state;
            if (pet.Energy == 100)
            {
                _sleepTimer?.Dispose();
                _sleepTimer = null;
                _timer?.Dispose();
                _timer = null;
            }
            pet.Energy += 5;
            UpdateDisplay();
            Console.Write("Press any key: ");
        }
        private void ExpensesDuringSleep(object state)
        {
            var pet = (Pet)state;
            if (pet.Hungry == 0)
            {
                _timer?.Dispose();
                _timer = null;
            }
            pet.Hungry -= 10;
            UpdateDisplay();
            Console.Write("Press any key: ");
        }

        public void Play()
        {
            Initialization();
            int enter = 0;
            while (enter != 4)
            {
                UpdateDisplay();
                var original_options = new[] { (Pets.Count() <= 0 ? "Create" : "Add"), "Feed", "Play", "Put to bed", "Exit" };
                var options = original_options;
                enter = ChoicePanel(options);

                switch (enter)
                {
                    case 0:
                        CreatePet();
                        break;
                    case 1:
                        if (Pets.Count() <= 0)
                        {
                            messageHandler += delegate {
                                Console.Write("You can't feed nothing");
                            };
                            break;
                        }
                        if (Pets[Index].Hungry >= 100)
                        {
                            messageHandler += delegate {
                                Console.Write($"{Pets[Index].Name} doesn't want to eat");
                            };
                            break;
                        }
                        if (Pets[Index].Energy <= 10)
                        {
                            messageHandler += delegate {
                                Console.Write($"{Pets[Index].Name} is too sleepy");
                            };
                            break;
                        }

                        Feed(Pets[Index]);
                        break;
                    case 2:
                        if (Pets.Count() <= 0)
                        {
                            messageHandler += delegate {
                                Console.Write("You can't play with nothing");
                            };
                            break;
                        }
                        if (Pets[Index].Energy <= 10)
                        {
                            messageHandler += delegate {
                                Console.Write($"{Pets[Index].Name} is too sleepy");
                            };
                            break;
                        }

                        PlayWithPet(Pets[Index]);
                        break;
                    case 3:
                        if (Pets.Count() <= 0)
                        {
                            messageHandler += delegate {
                                Console.Write("You can't put to sleep nothing");
                            };
                            break;
                        }
                        if (Pets[Index].Energy >= 80)
                        {
                            messageHandler += delegate {
                                Console.Write($"{Pets[Index].Name} doesn't want to sleep");
                            };
                            break;
                        }

                        PutToSleep(Pets[Index]);
                        break;
                }
            }
            Preservation();
            Console.Clear();
            Console.WriteLine("\nThank you\n\tfor playing\n\t\tADOPT ME\n\n\nEnding ...");
            Thread.Sleep(2000);
            Console.Clear();
        }

        private void UpdateDisplay()
        {
            Console.Clear();
            if (Pets.Count > 0)
            {
                Console.WriteLine($"\tAdopt me\n" +
                $"\n{Pets[Index].Species}: {Pets[Index].Name}" +
                $"\nhungry    - {Pets[Index].Hungry}" +
                $"\nhappiness - {Pets[Index].Happiness}" +
                $"\nenergy    - {Pets[Index].Energy}");
                messageHandler?.Invoke();
                messageHandler = null;
                Console.WriteLine("\n");
            }
            else
            {
                Console.WriteLine("\n\n\tADOPT ME\n");
                messageHandler?.Invoke();
                messageHandler = null;
                Console.WriteLine("\n");
            }
        }
        private int ChoicePanel(params string[] choices)
        {
            if (choices.Length <= 0) { return 0; }
            int index = 0;
            ConsoleKeyInfo choice = new();
            Console.CursorVisible = false;
            int startTop = Console.CursorTop;
            void DrawChoices()
            {
                for (int i = 0; i < choices.Length; i++)
                {
                    Console.SetCursorPosition(0, startTop + i);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, startTop + i);
                    if (index == i) { Console.Write(choices[i] + "  <--"); }
                    else { Console.Write(choices[i]); }
                }
            }
            DrawChoices();
            if (Pets.Count() == 0) { Console.WriteLine("\n\nNavigation: <, ^, v, > / W, A, S, D"); }


            while (choice.Key != ConsoleKey.Enter)
            {
                choice = Console.ReadKey(true);
                switch (choice.Key)
                {
                    case ConsoleKey.W or ConsoleKey.UpArrow:
                        index = (index == 0) ? choices.Length - 1 : index - 1;
                        break;
                    case ConsoleKey.S or ConsoleKey.DownArrow:
                        index = (index == choices.Length - 1) ? 0 : index + 1;
                        break;
                    case ConsoleKey.A or ConsoleKey.LeftArrow:
                        if (Pets.Count() == 0) { break; }
                        Index = (Index == 0) ? Pets.Count() - 1 : Index - 1;
                        UpdateDisplay();
                        break;
                    case ConsoleKey.D or ConsoleKey.RightArrow:
                        if (Pets.Count() == 0) { break; }
                        Index = (Index == Pets.Count() - 1) ? 0 : Index + 1;
                        UpdateDisplay();
                        break;
                }
                DrawChoices();
            }

            for (int i = 0; i < choices.Length; i++)
            {
                Console.SetCursorPosition(0, startTop + i);
                Console.Write(new string(' ', Console.WindowWidth));
            }
            Console.SetCursorPosition(0, startTop);
            Console.CursorVisible = true;
            return index;
        }

        private void Initialization()
        {
            if (SavesPath == null)
            {
                SavesPath = "saves.json";
            }
            if (!(File.Exists(SavesPath))) { return; }
            using (FileStream fs = new FileStream(SavesPath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fs))
            {
                string json_data = reader.ReadToEnd();
                if (json_data == "") { return; }
                var pets = JsonSerializer.Deserialize<Pet[]>(json_data);
                if (pets == null) { return; }
                foreach (var pet in pets)
                {
                    if (pet != null)
                    {
                        Pets.Add(pet);
                    }
                }
            }
        }
        private void Preservation()
        {
            if (Pets.Count <= 0)
            {
                return;
            }
            using (FileStream fs = new FileStream(SavesPath, FileMode.Create))
            {

                JsonSerializerOptions options = new()
                {
                    WriteIndented = true
                };
                JsonSerializer.SerializeAsync(fs, Pets, options);
            }
        }

    }
