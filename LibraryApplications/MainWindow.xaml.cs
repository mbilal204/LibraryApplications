using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static System.Formats.Asn1.AsnWriter;

namespace LibraryApplications
{
    public partial class MainWindow : Window
    {
        private List<string> generatedCallNumbers = new List<string>();
        private int score = 0;
        private int totalQuestion = 0;
        private int totalCorrectAns = 0;
        private int totalWrongAns = 0;

        // Dictionary to store call numbers and descriptions
        private Dictionary<string, string> callNumberDescriptions = new Dictionary<string, string>
        {
            { "000", "Wide and varied variety of facts and information that are widely accepted or understood, " },
            { "100", "The category for works that explore into the most fundamental questions about life, knowledge, and the scientific investigation of human thought and behavior." },
            { "200", "The material covers spirituality, religious rituals, and belief systems." },
            { "300", "Books addressing human society, culture, and behavior are covered in this course." },
            { "400", "Covers a variety of communication systems that individuals or organizations use to express meaning through speech, symbols, or gestures." },
            { "500", "Materials about geology, biology, and the natural world in class." },
            { "600", "A curriculum including literature on gadgets, instruments, devices  and practical skills." },
            { "700", "You may find books here about sports, hobbies, and the visual arts." },
            { "800", "Stories, novels, and other works of creative literature are covered here." },
            { "900", "Books on historical narratives and previous events, as well as studies of the Earth's physical characteristics, climate, human population, and interactions, belong here." },

        };
        private Dictionary<string, string> userAnswers = new Dictionary<string, string>();
        private bool taskAttempted;

        //Root Node 
        private DeweyNode rootDeweyNode;
        private DeweyNode currentQuestionNode; // Store the current question node
        private DeweyNode correctAnswerNode; // Store the correct answer node for comparison

        //Implementation of a start button to start the application. 
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            generatedCallNumbers = GenerateCallNumbers(10);
            //DisplayCallNumbers(generatedCallNumbers);
            submitButton.IsEnabled = true;

            ComboBoxItem typeItem = (ComboBoxItem)taskComboBox.SelectedItem;
            string value = typeItem.Content.ToString();
            if (value == "Finding call numbers")
            {
                DisplayRandomQuestion();
                pnlQuestion.Visibility = Visibility.Visible;
                pnlSummary.Visibility = Visibility.Hidden;
            }
            
        }

        //Generating the call numbers. 
        private List<string> GenerateCallNumbers(int count)
        {
            var random = new Random();
            var callNumbers = new List<string>();

            for (int i = 0; i < count; i++)
            {
                string deweyDecimal = $"{random.Next(100, 1000):D3}.{random.Next(10, 100):D2}";
                string authorSurname = GetRandomAuthorSurname();
                string callNumber = $"{deweyDecimal} {authorSurname.Substring(0, 3)}";
                callNumbers.Add(callNumber);
            }

            return callNumbers;
        }

        public MainWindow()
        {
            InitializeComponent();
            pnlQuestion.Visibility = Visibility.Hidden;
            pnlSummary.Visibility = Visibility.Hidden;
            // string filePath = "D:\\Bilal\\Development\\Projects\\WinForm\\LibraryApplications\\data.txt"; 
            //LoadDeweyDataFromFile(filePath);
            random = new Random();
        }

        //Combo box to store the tasks. Allows user to pick which task they want to do. 
        private void TaskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)taskComboBox.SelectedItem;
            string value = typeItem.Content.ToString();

            if (value != null)
            {
                if (value == "Replacing books")
                {
                    startButton.IsEnabled = true;
                    pnlQuestion.Visibility = Visibility.Hidden;
                    pnlSummary.Visibility = Visibility.Hidden;
                }
                else if (value == "Identifying Areas")
                {
                    if (!taskAttempted)
                    {
                        StartIdentifyingAreasTask();
                        startButton.IsEnabled = true;
                        // Enable the "Try Again" button if the task hasn't been attempted
                        tryAgainButton.IsEnabled = true;
                    }
                    pnlQuestion.Visibility = Visibility.Hidden;
                    pnlSummary.Visibility = Visibility.Hidden;
                }
                else if (value == "Finding call numbers")
                {
                    //StartFindingCallNumbersTask();
                    LoadCategoriesFromJson();
                    startButton.IsEnabled = true;
                }
            }
        }

        private void StartFindingCallNumbersTask()
        {
            try
            {
                string filePath = "C:\\Users\\lab_services_student\\source\\repos\\data.txt";
                List<Tuple<string, string>> deweyData = new List<Tuple<string, string>>();

                // Read data from the file and build the Dewey Decimal tree
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(','); // Split the line into CallNumber and Description
                        if (parts.Length == 2)
                        {
                            deweyData.Add(new Tuple<string, string>(parts[0], parts[1]));
                        }
                    }
                }

                // Initialize the root node
                rootDeweyNode = new DeweyNode { CallNumber = "000", Description = "General Knowledge" };

                // Build the Dewey Decimal tree from the loaded data
                foreach (var data in deweyData)
                {
                    AddNodeToTree(rootDeweyNode, data.Item1, data.Item2);
                }

                // Display the root node of the Dewey Decimal tree
                DisplayDeweyNode(rootDeweyNode);

                // Generate quiz question and display options
                GenerateQuizQuestion();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Handle any exceptions or errors during the task execution
            }
        }

        //Identifying Areas code 
        private void StartIdentifyingAreasTask()
        {
            var question = GenerateMatchingColumnQuestion();
            DisplayMatchingColumnQuestion(question);
        }

        private Dictionary<string, List<string>> GenerateMatchingColumnQuestion()
        {
            var random = new Random();
            var selectedCallNumbers = callNumberDescriptions.Keys.OrderBy(x => random.Next()).Take(4).ToList();
            var possibleAnswers = callNumberDescriptions.Values.OrderBy(x => random.Next()).Take(3).ToList();
            possibleAnswers.AddRange(selectedCallNumbers.Select(callNumber => callNumberDescriptions[callNumber]));
            possibleAnswers = possibleAnswers.OrderBy(x => random.Next()).ToList();

            var question = new Dictionary<string, List<string>>
            {
                { "CallNumbers", selectedCallNumbers },
                { "Descriptions", possibleAnswers }
            };

            return question;
        }

        private void DisplayMatchingColumnQuestion(Dictionary<string, List<string>> question)
        {
            callNumbersListView.Visibility = Visibility.Collapsed;
            submitButton.IsEnabled = true;
            resultLabel.Visibility = Visibility.Visible;
            lblScore.Visibility = Visibility.Visible;

            userAnswers.Clear();

            int row = 0;
            foreach (var callNumber in question["CallNumbers"])
            {
                Label label = new Label
                {
                    Content = callNumber,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                };
                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);
                matchingGrid.Children.Add(label);
                row++;
            }

            List<string> shuffledDescriptions = question["Descriptions"].OrderBy(x => Guid.NewGuid()).ToList();
            row = 0;
            foreach (var description in shuffledDescriptions)
            {
                Button button = new Button
                {
                    Content = description,
                    FontSize = 12,
                    Padding = new Thickness(8),
                };
                button.Click += (sender, e) =>
                {
                    if (userAnswers.Count < question["CallNumbers"].Count)
                    {
                        if (!userAnswers.ContainsKey(description))
                        {
                            userAnswers.Add(description, null);
                            button.IsEnabled = false;
                        }
                    }
                };

                Grid.SetRow(button, row);
                Grid.SetColumn(button, 1);
                matchingGrid.Children.Add(button);
                row++;
            }
        }
        //Submit Button 
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            pnlQuestion.Visibility = Visibility.Hidden;
            pnlSummary.Visibility = Visibility.Visible;

            lblTotalQstn.Content = new TextBlock
            {
                Inlines = {

                        new Run { Text = "Total Questions = " },
                        new Run { Text =  Convert.ToString(totalQuestion), FontWeight = FontWeights.Bold }
                         }
            };

            lblCorrectAns.Content = new TextBlock
            {
                Inlines = {

                        new Run { Text = "Total Correct Ans = " },
                        new Run { Text =  Convert.ToString(totalCorrectAns), FontWeight = FontWeights.Bold }
                         }
            };

            lblWrongAns.Content = new TextBlock
            {
                Inlines = {

                        new Run { Text = "Total Wrong Ans = " },
                        new Run { Text =  Convert.ToString(totalWrongAns), FontWeight = FontWeights.Bold }
                         }
            };

            //int correctCount = 0;

            //foreach (var callNumber in userAnswers.Values)
            //{
            //    if (callNumberDescriptions.ContainsKey(callNumber))
            //    {
            //        correctCount++;
            //    }
            //}

            //int totalQuestions = userAnswers.Count;

            //if (totalQuestions > 0)
            //{
            //    score += (correctCount * 100) / totalQuestions;
            //    resultLabel.Content = $"You answered {correctCount} out of {totalQuestions} correctly!";
            //}
            //else
            //{
            //    resultLabel.Content = "You didn't answer any questions!";
            //}

            //lblScore.Content = $"Score: {score}";
            //resultLabel.Visibility = Visibility.Visible;
            //lblScore.Visibility = Visibility.Visible;
            //submitButton.IsEnabled = false;
        }
        //Try Again Button
        private void TryAgainButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset the task to allow the user to try again
            pnlQuestion.Visibility = Visibility.Visible;
            pnlSummary.Visibility = Visibility.Hidden;
           // ResetIdentifyingAreasTask();
            ResetQuestionsSummary();
            ClearRadioButtons();
        }
        private void ResetIdentifyingAreasTask()
        {
            // Clear the previous matching question and user answers
            matchingGrid.Children.Clear();
            userAnswers.Clear();

            // Re-enable the "Start Task" button to restart the task
            startButton.IsEnabled = true;

            resultLabel.Visibility = Visibility.Hidden;
            lblScore.Visibility = Visibility.Hidden;

            // Enable the "Try Again" button 
            tryAgainButton.IsEnabled = true;
        }

        //Replacing Books code 
        private string GetRandomDeweyDecimal()
        {
            string[] deweyCategories = { "000", "100", "200", "300", "400", "500", "600", "700", "800", "900" };
            var random = new Random();
            double deweyDecimal = random.NextDouble() * (1000 - 001) + 001; // Generate a random Dewey Decimal number between 001 and 1000
            return deweyDecimal.ToString("0.00"); // Format as a Dewey Decimal number with two decimal places
        }

        //Generating the random author surnames.
        private string GetRandomAuthorSurname()
        {
            //First 3 letters of authors surnames.
            string[] surnames = { "JONES", "SMITH", "WILLIAMS", "THOMAS ", "ADAMS", "HARRY", "NAIOO", "MILLER", "DAVIS", "SCOTT" };
            var random = new Random();
            return surnames[random.Next(surnames.Length)];
        }
        //Display the call numbers.
        private void DisplayCallNumbers(List<string> callNumbers)
        {
            callNumbersListView.Items.Clear();
            foreach (var callNumber in callNumbers)
            {
                callNumbersListView.Items.Add(callNumber);
            }
            callNumbersListView.Visibility = Visibility.Visible;
        }
        private bool IsCorrectOrder(List<string> userOrder)
        {
            //Bubble sort algorithm to check the order.
            for (int i = 0; i < userOrder.Count - 1; i++)
            {
                for (int j = 0; j < userOrder.Count - i - 1; j++)
                {
                    if (string.Compare(userOrder[j], userOrder[j + 1]) > 0)
                    {
                        // Swap elements if they are out of order
                        string temp = userOrder[j];
                        userOrder[j] = userOrder[j + 1];
                        userOrder[j + 1] = temp;
                    }
                }
            }
            //Comparing the sorted user order with the original generated order
            return userOrder.SequenceEqual(generatedCallNumbers);
        }

        //Part 3 

        private void LoadDeweyDataFromFile(string filePath)
        {
            List<Tuple<string, string>> deweyData = new List<Tuple<string, string>>();

            try
            {
                // Read the file line by line and populate deweyData
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(','); // Split the line into CallNumber and Description
                        if (parts.Length == 2)
                        {
                            deweyData.Add(new Tuple<string, string>(parts[0], parts[1]));
                        }
                    }
                }

                // Initialize the root node
                rootDeweyNode = new DeweyNode { CallNumber = "000", Description = "Generalities" };

                // Build the Dewey Decimal tree from the loaded data
                foreach (var data in deweyData)
                {
                    AddNodeToTree(rootDeweyNode, data.Item1, data.Item2);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions or errors during file reading
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        private void AddNodeToTree(DeweyNode parentNode, string callNumber, string description)
        {
            string[] parts = callNumber.Split('.'); // Split the call number into parts
            DeweyNode currentNode = parentNode;

            foreach (string part in parts)
            {
                DeweyNode childNode = currentNode.Children.FirstOrDefault(node => node.CallNumber == part);

                if (childNode == null)
                {
                    childNode = new DeweyNode { CallNumber = part, Description = description };
                    currentNode.Children.Add(childNode);
                }

                currentNode = childNode;
            }
        }
        private Stack<DeweyNode> nodeStack = new Stack<DeweyNode>(); // Stack to keep track of nodes for navigation

        private Panel treeDisplayPanel; // Declaration at the class level
        private void DisplayDeweyNode(DeweyNode node)
        {
            // Clear previous content before displaying new node information
            treeDisplayPanel.Children.Clear();

            // Display the current node information
            Label nodeLabel = new Label
            {
                Content = $"{node.CallNumber}: {node.Description}",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5),
            };
            treeDisplayPanel.Children.Add(nodeLabel);

            // Display children nodes (if any)
            foreach (DeweyNode childNode in node.Children)
            {
                Button childButton = new Button
                {
                    Content = $"{childNode.CallNumber}: {childNode.Description}",
                    Margin = new Thickness(5),
                };
                childButton.Click += (sender, e) =>
                {
                    nodeStack.Push(node); // Push the current node to the stack for back navigation
                    DisplayDeweyNode(childNode); // Display the selected child node
                };

                treeDisplayPanel.Children.Add(childButton);
            }

            // Back button to navigate back to the parent node
            if (nodeStack.Count > 0)
            {
                Button backButton = new Button
                {
                    Content = "Back",
                    Margin = new Thickness(5),
                };
                backButton.Click += (sender, e) =>
                {
                    DeweyNode parentNode = nodeStack.Pop(); // Retrieve the parent node from the stack
                    DisplayDeweyNode(parentNode); // Display the parent node
                };

                treeDisplayPanel.Children.Add(backButton);
            }
        }

        private void GenerateQuizQuestion()
        {
            // Randomly select a third level entry from the data
            List<DeweyNode> thirdLevelNodes = GetThirdLevelNodes(rootDeweyNode);
            currentQuestionNode = thirdLevelNodes[new Random().Next(thirdLevelNodes.Count)];

            // Display description of the selected node to the user
            DisplayQuestionDescription(currentQuestionNode.Description);

            // Display four top-level options to the user (one correct and three incorrect)
            DisplayOptions();
        }

        private List<DeweyNode> GetThirdLevelNodes(DeweyNode node)
        {
            List<DeweyNode> thirdLevelNodes = new List<DeweyNode>();

            foreach (DeweyNode child in node.Children)
            {
                foreach (DeweyNode grandChild in child.Children)
                {
                    if (grandChild.Children.Count == 0)
                    {
                        thirdLevelNodes.Add(grandChild);
                    }
                }
            }

            return thirdLevelNodes;
        }

        private void DisplayQuestionDescription(string description)
        {
            // Assuming you have a text block named 'questionDescriptionTextBlock' in your XAML file
            questionDescriptionTextBlock.Text = description;
        }

        private void DisplayOptions()
        {
            // Generate four options (one correct, three incorrect)
            List<DeweyNode> options = GenerateOptions();

            // Display options to the user in numerical order by call number
            DisplayOptionsToUser(options);
        }

        private List<DeweyNode> GenerateOptions()
        {
            // Generate options (one correct, three incorrect)
            List<DeweyNode> options = new List<DeweyNode>();
            correctAnswerNode = currentQuestionNode;

            while (options.Count < 4)
            {
                DeweyNode optionNode = GetRandomTopLevelNode();

                if (optionNode != null && !options.Contains(optionNode))
                {
                    options.Add(optionNode);
                }
            }

            return options;
        }

        private DeweyNode GetRandomTopLevelNode()
        {
            // Get a random top-level node for generating options
            List<DeweyNode> topLevelNodes = rootDeweyNode.Children;
            return topLevelNodes[new Random().Next(topLevelNodes.Count)];
        }

        private void DisplayOptionsToUser(List<DeweyNode> options)
        {
            // Assuming you have a StackPanel named optionsPanel in your XAML file
            optionsPanel.Children.Clear(); // Clear previous options

            foreach (DeweyNode option in options.OrderBy(opt => opt.CallNumber))
            {
                Button optionButton = new Button
                {
                    Content = $"{option.CallNumber}: {option.Description}",
                    Tag = option // Store the DeweyNode object as the Tag for later retrieval
                };

                // Add a click event handler to the button for user interaction
                optionButton.Click += OptionButton_Click;

                optionsPanel.Children.Add(optionButton); // Add the button to the UI
            }
        }

        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the DeweyNode object associated with the clicked button
            DeweyNode selectedNode = (sender as Button)?.Tag as DeweyNode;

            if (selectedNode != null)
            {
                UserSelectedOption(selectedNode); // Handle the user's selected option
            }
        }

        // Handle user selection of an option
        private void UserSelectedOption(DeweyNode selectedNode)
        {
            if (selectedNode == correctAnswerNode)
            {
                // If the user selects the correct option, show options from the next level
                DisplayNextLevelOptions();
            }
            else
            {
                // If the user selects the wrong option, indicate and move to the next question
                IndicateWrongAnswer();
                GenerateQuizQuestion();
            }
        }

        private void DisplayNextLevelOptions()
        {
            // Check if the current question node has children (next level options)
            if (currentQuestionNode.Children.Count > 0)
            {
                // Display options from the next level
                List<DeweyNode> nextLevelOptions = currentQuestionNode.Children;
                DisplayOptionsToUser(nextLevelOptions);
                // Update the current question node to the next level
                currentQuestionNode = nextLevelOptions[0]; // For example, consider the first child node for simplicity
                correctAnswerNode = currentQuestionNode; // Update the correct answer node
            }
            else
            {
                // Indicate that the user has reached the most detailed level
                IndicateMostDetailedLevelReached();
                score += 50; // Add bonus points for reaching the next level
                UpdateScoreUI();
            }
        }

        private void IndicateMostDetailedLevelReached()
        {
            throw new NotImplementedException();
        }

        private void IndicateWrongAnswer()
        {
            // Display a message indicating the wrong answer to the user
            MessageBox.Show("Incorrect answer. Please try again.", "Wrong Answer", MessageBoxButton.OK, MessageBoxImage.Information);
            // You can add any other UI updates or logic you want to perform when the answer is wrong
        }

        //Gamification 

        private void IncreaseScore()
        {
            // Increase the user's score for a correct answer
            score += 100; // Increment the score by a certain amount (e.g., 100 points per correct answer)
            UpdateScoreUI(); // Update the score displayed in the UI
        }

        private void UpdateScoreUI()
        {
            //scoreTextBlock.Text = $"Score: {score}";
        }



        // Bilal Code below 

        private List<Category> categories;
        private Random random;
        private Question currentQuestion;
        private bool IsSubCategory;

        private void LoadCategoriesFromJson()
        {
            string fileName = "data.json";
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            string jsonText = File.ReadAllText(jsonFilePath);
            RootObject root = JsonConvert.DeserializeObject<RootObject>(jsonText);
            categories = root.Categories;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (optA.IsChecked== true || optB.IsChecked == true || optC.IsChecked == true || optD.IsChecked == true)
            {
                IsSubCategory = false;
                ClearRadioButtons();
                DisplayRandomQuestion();
            }
            else
            {
                MessageBox.Show("Please select one option then click on Next button! ", "Error", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }
        private void DisplayRandomQuestion()
        {
            lblResult.Content = "";
            totalQuestion++;
            if (categories != null && categories.Count > 0)
            {
                Category randomCategory = categories[random.Next(categories.Count)];
                if (randomCategory.Subcategories != null && randomCategory.Subcategories.Count > 0)
                {
                    Subcategory randomSubcategory = randomCategory.Subcategories[random.Next(randomCategory.Subcategories.Count)];
                    if (randomSubcategory.Topics != null && randomSubcategory.Topics.Count > 0)
                    {
                        Topic currentTopic = randomSubcategory.Topics[random.Next(randomSubcategory.Topics.Count)];
                        lblQuestion.Content = new TextBlock
                        {
                            Inlines = {
                        new Run { Text = currentTopic.Name, FontWeight = FontWeights.Bold },
                        new Run { Text = " belongs to which category ?" }
                    }
                        };

                        // Create a new Question object
                        currentQuestion = new Question
                        {
                            Id = currentTopic.Id, // Set Id if needed
                            Name = currentTopic.Name,
                            CorrectOption = new Option { Id = randomCategory.Id, Name = randomCategory.Name }, // Set CorrectOption if needed
                            Options = null // Set Options if needed
                        };

                        string txtQuestion = currentQuestion.Name;
                        List<RadioButton> options = new List<RadioButton> { optA, optB, optC, optD };
                        Shuffle(options);

                        bool correctCategoryAssigned = false;

                        foreach (RadioButton option in options)
                        {
                            if (!correctCategoryAssigned)
                            {
                                option.Content = randomCategory.Name;
                                correctCategoryAssigned = true;
                            }
                            else
                            {
                                string randomCategoryName;
                                do
                                {
                                    randomCategoryName = categories[random.Next(categories.Count)].Name;
                                } while (randomCategoryName == randomCategory.Name || options.Any(o => o.Content != null && o.Content.ToString() == randomCategoryName));
                                option.Content = randomCategoryName;
                            }
                        }
                    }
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton selectedOption = (RadioButton)sender;
            if (selectedOption.Content.ToString() == currentQuestion.CorrectOption.Name)
            {
                // Correct
                score++;
                totalCorrectAns++;
                lblResult.Content = "Correct";
                lblScore.Content = $"Score: {score}";
                if (!IsSubCategory)
                {
                    DisplaySubcategories();
                }
            }
            else
            {
                totalWrongAns++;
                lblResult.Content = new TextBlock
                {
                    Inlines = {

                        new Run { Text = "Wrong! Correct Answer is= " },
                        new Run { Text = currentQuestion.CorrectOption.Name, FontWeight = FontWeights.Bold }
                         }
                };
            }
            // DisplayRandomQuestion();
        }

        private void DisplaySubcategories()
        {
            lblResult.Content = "";
            IsSubCategory = true;
            // Clear previous options
            ClearRadioButtons();

            // Retrieve subcategories of the correct category
            Category correctCategory = categories.First(c => c.Name == currentQuestion.CorrectOption.Name);
            List<Subcategory> subcategories = correctCategory.Subcategories;

            lblQuestion.Content = new TextBlock
            {
                Inlines = {
                        new Run { Text = currentQuestion.Name, FontWeight = FontWeights.Bold },
                        new Run { Text = " belongs to which sub-category ?" }
                    }
            };


            // Find the subcategory that contains the specific topic
            Subcategory selectedSubcategory = subcategories.FirstOrDefault(sub => sub.Topics.Any(topic => topic.Id == currentQuestion.Id));
            if (selectedSubcategory != null)
            {
                string selectedSubcategoryName = selectedSubcategory.Name;
                currentQuestion.CorrectOption = new Option
                {
                    Id = selectedSubcategory.Id,
                    Name = selectedSubcategoryName
                };
            }

            // Display subcategories on radio buttons
            List<RadioButton> options = new List<RadioButton> { optA, optB, optC, optD };

            // Shuffle the options
            Shuffle(options);

            // Replace one of the options with the correct one
            int correctOptionIndex = random.Next(options.Count);
            options[correctOptionIndex].Content = currentQuestion.CorrectOption.Name;

            // Display the rest of the subcategory options
            for (int i = 0; i < options.Count && i < subcategories.Count; i++)
            {
                // Skip the correct option, as it has already been assigned
                if (i != correctOptionIndex)
                {
                    options[i].Content = subcategories[i].Name;
                    options[i].Visibility = Visibility.Visible; // Ensure the option is visible
                }
            }
        }



        private void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void ResetQuestionsSummary()
        {
            score = 0;
            totalCorrectAns = 0;
            totalWrongAns = 0;
            totalQuestion = 0;
            lblScore.Content = "Score: 0";
        }
        private void ClearRadioButtons()
        {
            optA.IsChecked = false;
            optB.IsChecked = false;
            optC.IsChecked = false;
            optD.IsChecked = false;
        }
    }

    public class Topic
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Subcategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Topic> Topics { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Subcategory> Subcategories { get; set; }
    }

    public class RootObject
    {
        public List<Category> Categories { get; set; }
    }
    public class Question
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Option CorrectOption { get; set; }
        public List<Option> Options { get; set; }
    }

    public class Option
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
