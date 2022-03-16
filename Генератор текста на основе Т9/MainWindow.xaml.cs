using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Генератор_текста_на_основе_Т9
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        DatabaseContext db = new DatabaseContext();
        //List of verified words
        List<string> ListCheckedWords = new List<string>();
        //List of verified words for FindChainWord
        List<string> ListCheckedWordsAsync = new List<string>();
        //List of words from the database
        List<Word> ListWords = new List<Word>();

        //Recursion return depth level 
        public int CloseDeeplvl = -1;
        public int CountOfAsync = 0;

        //Number of threads completed
        public int ThreadFinish = 0;

        private void btEat_Click(object sender, RoutedEventArgs e)
        {
            ListWords = db.Words.ToList();
            TextRange textRange = new TextRange(
                rtbText.Document.ContentStart,
                rtbText.Document.ContentEnd
                );
            string[] words = textRange.Text.ToLower().Replace("\n",".").Replace("\r", ".").Split(' ');

            int NumThreads = 8;

            int CountPart = words.Length / NumThreads;

            
            Thread t1 = new Thread(() => dbText(words[(CountPart * (0))..(CountPart * (1))]));
            Thread t2 = new Thread(() => dbText(words[(CountPart * (1))..(CountPart * (2))]));
            Thread t3 = new Thread(() => dbText(words[(CountPart * (2))..(CountPart * (3))]));
            Thread t4 = new Thread(() => dbText(words[(CountPart * (3))..(CountPart * (4))]));
            Thread t5 = new Thread(() => dbText(words[(CountPart * (4))..(CountPart * (5))]));
            Thread t6 = new Thread(() => dbText(words[(CountPart * (5))..(CountPart * (6))]));
            Thread t7 = new Thread(() => dbText(words[(CountPart * (6))..(CountPart * (7))]));
            Thread t8 = new Thread(() => dbText(words[(CountPart * (7))..(CountPart * (8))]));
            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            t5.Start();
            t6.Start();
            t7.Start();
            t8.Start();



            MessageBox.Show("The absorption process has begun");
        }

        private async void brGenerate_Click(object sender, RoutedEventArgs e)
        {
            var dbWords = db.Words.OrderByDescending(p => p.Count);
            ListWords = db.Words.ToList();
            ListCheckedWords.Clear();
            ListCheckedWordsAsync.Clear();
            CloseDeeplvl = -1;

            string[] words = tbWord.Text.Split(' ');
            string tempWord = words.Last();
            string text = tempWord;

            //Простая генерация на основе наипопулярных слов
            if (words.Length == 1)
                for (int i = 0; i < 50; i++)
                {
                    var word = dbWords.FirstOrDefault(p => p.WordFirst == tempWord);

                    tempWord = word.WordSecond;
                    text += " " + tempWord;
                }

            //Пробежка по цепочке слов
            if (words.Length > 1)
            {
                text = "";
                for (int i = 0; i < words.Length-1; i++)
                {
                    var LuckyWord = dbWords.FirstOrDefault(p => p.WordFirst == words[i] & p.WordSecond == words[i+1]);
                    if (LuckyWord == null)
                    {
                        text += await FindChainWord(words[i], words[i + 1]);
                    }
                    else
                        text += LuckyWord.WordFirst + " " + LuckyWord.WordSecond;
                }
                    
            }

            MessageBox.Show(text);
        }

        /// <summary>
        /// Search the word chain in the tree between the FirstWordand the SearchedWord
        /// </summary>
        /// <param name="FirstWord"></param>
        /// <param name="SearchedWord"></param>
        /// <param name="DeepLvl"></param>
        /// <returns></returns>
        private async Task<string> FindChainWord(string FirstWord, string SearchedWord, int DeepLvl = 0)
        {
            CountOfAsync++;
            string text = FirstWord;
            if (ListWords.FirstOrDefault(p => p.WordFirst == FirstWord & p.WordSecond == SearchedWord) == null)
            {
                var DependWords = ListWords.Where(p => p.WordFirst == FirstWord);
                ListCheckedWords.Add(FirstWord);
                bool WordFound = false;
                //Проверка первых связанных слов
                foreach (var Word in DependWords.Select(p => p.WordSecond))
                {
                    if (DeepLvl >= CloseDeeplvl && CloseDeeplvl != -1)
                    {
                        CountOfAsync--;
                        return text;
                    }
                        
                    if (!ListCheckedWords.Contains(Word) & !WordFound)
                        if (ListWords.FirstOrDefault(p => p.WordFirst == Word & p.WordSecond == SearchedWord) != null)
                        {
                            WordFound = true;
                            text += " " + Word + " " + SearchedWord;
                            CloseDeeplvl = DeepLvl;
                        }
                        else
                        {
                            ListCheckedWords.Add(Word);
                        }

                }
                //In case there is no match among the dependent WordSecond from WordFirst
                if (!WordFound)
                    foreach (var Word in DependWords.Select(p => p.WordSecond))
                    {
                        if (DeepLvl >= CloseDeeplvl && CloseDeeplvl != -1)
                        {
                            CountOfAsync--;
                            return text;
                        }
                        //Рекурсия
                        if (!ListCheckedWordsAsync.Contains(Word))
                        {
                            ListCheckedWordsAsync.Add(Word);
                            text += " " + await FindChainWord(Word, SearchedWord, DeepLvl + 1);
                        }
                            

                        if (text.Contains(SearchedWord))
                        {
                            CloseDeeplvl = DeepLvl;
                            CountOfAsync--;
                            return text;
                        }
                    }
            }
            else
            {
                CountOfAsync--;
                text += " " + SearchedWord;
            }


            CountOfAsync--;
            return text;
        }

        //Работать с копией бд из ОЗУ. Сделать как раз проверку на количество законченных потоков и после всех начать глобальное сохранение в бд
        //public async void dbText(string[] words)
        //{
        //    DatabaseContext db1 = new DatabaseContext();
        //    string LastWord = "";
        //    foreach (string word in words)
        //    {
        //        if (LastWord == "")
        //            LastWord = word;
        //        else
        //        {

        //            var dbWord = db1.Words.FirstOrDefault(p => p.WordFirst == LastWord && p.WordSecond == word);
        //            if (dbWord == null)
        //            {
        //                db1.Words.Add(new Word() { WordFirst = LastWord, WordSecond = word, Count = 1 });
        //            }
        //            else
        //            {
        //                dbWord.Count++;

        //            }
        //            LastWord = word;
        //        }
        //    }
        //    await db1.SaveChangesAsync();
        //}

        /// <summary>
        /// Creates a list of words and then adds it to the database
        /// </summary>
        /// <param name="words"></param>
        public async void dbText(string[] words)
        {
            List<Word> ListAddedWords = new List<Word>();
            string LastWord = "";
            foreach (string word in words)
            {
                if (LastWord == "")
                    LastWord = word;
                else
                {

                    var ListWord = ListAddedWords.FirstOrDefault(p => p.WordFirst == LastWord && p.WordSecond == word);
                    if (ListWord == null)
                    {
                        ListAddedWords.Add(new Word() { WordFirst = LastWord, WordSecond = word, Count = 1 });
                    }
                    else
                    {
                        ListWord.Count++;

                    }
                    LastWord = word;
                }
            }
            

            DatabaseContext db1 = new DatabaseContext();
            foreach (var word in ListAddedWords)
            {
                var ListWord = ListWords.FirstOrDefault(p => p.WordFirst == word.WordFirst && p.WordSecond == word.WordSecond);

                var dbWord = db1.Words.FirstOrDefault(p => p.WordFirst == word.WordFirst && p.WordSecond == word.WordSecond);
                if (dbWord == null)
                {
                    if (ListWord != null)
                        db1.Words.Add(new Word() { WordFirst = word.WordFirst, WordSecond = word.WordSecond, Count = word.Count + ListWord.Count });
                    else
                        db1.Words.Add(new Word() { WordFirst = word.WordFirst, WordSecond = word.WordSecond, Count = word.Count});
                }
                else
                {
                    dbWord.Count = word.Count + ListWord.Count;

                }
            }
            ThreadFinish++;
            await db1.SaveChangesAsync();
            if (ThreadFinish == 8)
                MessageBox.Show("Конец!");
            ThreadFinish = 0;
        }
    }
}
