using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Question
{
    public string QuestionText { get; set; }
    public List<string> Answers { get; set; }
    public int CorrectAnswerIndex { get; set; }

    public Question(string questionText, List<string> answers, int correctAnswerIndex)
    {
        QuestionText = questionText;
        Answers = answers ?? new List<string>();
        CorrectAnswerIndex = correctAnswerIndex;
    }
}

public class Quiz
{
    public List<Question> Questions { get; set; }

    public Quiz()
    {
        Questions = new List<Question>();
    }

    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("A megadott fájl nem létezik.", filePath);
        }

        string[] lines = File.ReadAllLines(filePath);
        int index = 0;
        while (index < lines.Length)
        {
            // Skip empty lines
            while (index < lines.Length && string.IsNullOrWhiteSpace(lines[index]))
            {
                index++;
            }

            if (index >= lines.Length)
            {
                break;
            }

            // Read question
            string questionText = lines[index].Trim();
            index++;
            if (string.IsNullOrWhiteSpace(questionText))
            {
                continue; // Skip empty questions
            }

            // Read answers until we hit a number or empty line
            List<string> answers = new List<string>();
            while (index < lines.Length && !string.IsNullOrWhiteSpace(lines[index]) && !int.TryParse(lines[index].Trim(), out _))
            {
                answers.Add(lines[index].Trim());
                index++;
            }

            // Read correct answer index
            if (index >= lines.Length || string.IsNullOrWhiteSpace(lines[index]))
            {
                throw new FormatException($"Hiányzik a helyes válasz indexe a következő kérdéshez: {questionText}");
            }

            string indexLine = lines[index].Trim();
            if (!int.TryParse(indexLine, out int correctAnswerIndex) || correctAnswerIndex < 0 || correctAnswerIndex >= answers.Count)
            {
                throw new FormatException($"Érvénytelen helyes válasz index '{indexLine}' a következő kérdéshez: {questionText}");
            }
            index++;

            // Add the question to the list
            Questions.Add(new Question(questionText, answers, correctAnswerIndex));
        }
    }

    public void RunQuiz()
    {
        // Shuffle questions
        Random rng = new Random();
        var shuffledQuestions = Questions.OrderBy(q => rng.Next()).ToList();

        int score = 0;
        int totalQuestions = shuffledQuestions.Count;

        foreach (var question in shuffledQuestions)
        {
            Console.WriteLine($"\nKérdés: {question.QuestionText.Replace("\\n", "\n")}");


            // Shuffle answers
            var answerIndices = Enumerable.Range(0, question.Answers.Count).OrderBy(_ => rng.Next()).ToList();
            var shuffledAnswers = answerIndices.Select(i => question.Answers[i]).ToList();
            int correctAnswerPosition = answerIndices.IndexOf(question.CorrectAnswerIndex);
            Console.WriteLine();

            // Display answers with letters (A, B, C, etc.)
            for (int j = 0; j < shuffledAnswers.Count; j++)
            {
                char letter = (char)('A' + j);
                Console.WriteLine($"  {letter}. {shuffledAnswers[j]}");
            }

            // Get user input as a letter
            Console.Write("\nAdd meg a válasz betűjelét: ");
            string input = Console.ReadLine()?.Trim().ToUpper();
            bool isValid = !string.IsNullOrEmpty(input) && input.Length == 1 && input[0] >= 'A' && input[0] < 'A' + shuffledAnswers.Count;

            if (isValid && (input[0] - 'A') == correctAnswerPosition)
            {
                Console.WriteLine("Helyes!");
                score++;
            }
            else
            {
                char correctLetter = (char)('A' + correctAnswerPosition);
                Console.WriteLine($"Helytelen. A helyes válasz: {shuffledAnswers[correctAnswerPosition]} ({correctLetter})");
            }
        }

        Console.WriteLine($"\nKvíz vége! Eredményed: {score}/{totalQuestions}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Quiz quiz = new Quiz();
        try
        {
            quiz.LoadFromFile("questions.txt");
            do
            {
                quiz.RunQuiz();
                Console.Write("Szeretnéd újra kitölteni a kvízt? (I/N): ");
                string response = Console.ReadLine()?.Trim().ToUpper();
                if (response != "I")
                {
                    break;
                }
                Console.Clear();
            } while (true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hiba: {ex.Message}");
        }
    }
}