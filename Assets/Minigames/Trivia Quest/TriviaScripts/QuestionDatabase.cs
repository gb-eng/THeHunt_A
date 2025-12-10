using System.Collections.Generic;
using UnityEngine;

public class QuestionDatabase : MonoBehaviour
{
    public static QuestionDatabase Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<Question> allQuestions = new List<Question>
    {
        new Question
        {
            questionText = "What is Taal, Batangas known as?",
            optionA = "Heritage Town",
            optionB = "Modern City",
            optionC = "Beach Resort",
            correctAnswer = "A",
            explanation = "Taal is a Heritage Town with many historical buildings and rich culture."
        },
        new Question
        {
            questionText = "What is the name of the famous big church in Taal?",
            optionA = "Taal Cathedral",
            optionB = "Taal Basilica",
            optionC = "Taal Chapel",
            correctAnswer = "B",
            explanation = "The Taal Basilica, also called the Basilica of Saint Martin of Tours, is the largest church in Asia."
        },
        new Question
        {
            questionText = "Who is known as the 'Mother of the Philippine Flag'?",
            optionA = "Gabriela Silang",
            optionB = "Marcela Agoncillo",
            optionC = "Melchora Aquino",
            correctAnswer = "B",
            explanation = "Marcela Agoncillo from Taal sewed the first Philippine flag in Hong Kong."
        },
        new Question
        {
            questionText = "What did Marcela Agoncillo create for the Philippines?",
            optionA = "National Anthem",
            optionB = "Philippine Flag",
            optionC = "Philippine Map",
            correctAnswer = "B",
            explanation = "She sewed the first Philippine flag with the help of her daughter and Delfina Herbosa."
        },
        new Question
        {
            questionText = "What is Casa Real in Taal?",
            optionA = "A shopping mall",
            optionB = "A historic government building",
            optionC = "A modern hotel",
            correctAnswer = "B",
            explanation = "Casa Real was an old Spanish government building that served as the town hall."
        },
        new Question
        {
            questionText = "Who was Felipe Agoncillo?",
            optionA = "A Filipino diplomat and lawyer",
            optionB = "A Spanish governor",
            optionC = "A church priest",
            correctAnswer = "A",
            explanation = "Felipe Agoncillo was a lawyer and diplomat who represented the Philippines abroad."
        },
        new Question
        {
            questionText = "The Apacible family of Taal is known for?",
            optionA = "Making shoes",
            optionB = "Their contribution to Philippine history and politics",
            optionC = "Building churches",
            correctAnswer = "B",
            explanation = "The Apacibles were an influential family with members who served in government and contributed to Philippine independence."
        },
        new Question
        {
            questionText = "Who was Maria Orosa from Taal?",
            optionA = "A food scientist and war heroine",
            optionB = "A teacher",
            optionC = "A singer",
            correctAnswer = "A",
            explanation = "Maria Orosa was a famous food scientist who invented banana ketchup and helped feed people during World War II."
        },
        new Question
        {
            questionText = "What famous product did Maria Orosa invent?",
            optionA = "Soy sauce",
            optionB = "Banana ketchup",
            optionC = "Fish sauce",
            correctAnswer = "B",
            explanation = "Maria Orosa invented banana ketchup as a local alternative to tomato ketchup."
        },
        new Question
        {
            questionText = "What can you find at Taal Market?",
            optionA = "Only vegetables",
            optionB = "Local products and delicacies",
            optionC = "Only fish",
            correctAnswer = "B",
            explanation = "Taal Market offers various local products, fresh food, and the town's famous delicacies."
        },
        new Question
        {
            questionText = "What is a famous delicacy made in Taal from taro?",
            optionA = "Buko pie",
            optionB = "Gabi (taro) products",
            optionC = "Suman",
            correctAnswer = "B",
            explanation = "Taal is famous for its taro or gabi products like pastillas de gabi and other sweets."
        },
        new Question
        {
            questionText = "What is 'Panutsa'?",
            optionA = "A type of rice cake",
            optionB = "A peanut brittle candy",
            optionC = "A noodle dish",
            correctAnswer = "B",
            explanation = "Panutsa is a crunchy peanut brittle candy that Taal is famous for."
        },
        new Question
        {
            questionText = "What traditional embroidery is Taal known for?",
            optionA = "Calado",
            optionB = "Cross-stitch",
            optionC = "Crochet",
            correctAnswer = "A",
            explanation = "Calado or 'Burdang Taal' is the traditional delicate embroidery art of Taal."
        },
        new Question
        {
            questionText = "What type of knife is Taal and Batangas famous for?",
            optionA = "Kitchen knife",
            optionB = "Balisong (butterfly knife)",
            optionC = "Hunting knife",
            correctAnswer = "B",
            explanation = "Batangas province, including Taal, is famous for the balisong or butterfly knife."
        },
        new Question
        {
            questionText = "The Taal Basilica is dedicated to which saint?",
            optionA = "Saint Joseph",
            optionB = "Saint Martin of Tours",
            optionC = "Saint Peter",
            correctAnswer = "B",
            explanation = "The basilica is dedicated to Saint Martin of Tours, the patron saint of Taal."
        },
        new Question
        {
            questionText = "What style are the old houses in Taal called?",
            optionA = "Nipa huts",
            optionB = "Bahay na Bato",
            optionC = "Modern houses",
            correctAnswer = "B",
            explanation = "Bahay na Bato means 'stone house' - the traditional Spanish-Filipino style homes in Taal."
        },
        new Question
        {
            questionText = "When is Taal's town fiesta celebrated?",
            optionA = "November 11",
            optionB = "December 25",
            optionC = "June 12",
            correctAnswer = "A",
            explanation = "Taal celebrates its fiesta every November 11, the feast day of Saint Martin of Tours."
        },
        new Question
        {
            questionText = "What makes Taal a heritage town?",
            optionA = "It has many beaches",
            optionB = "It has preserved historic buildings and culture",
            optionC = "It has modern shopping malls",
            correctAnswer = "B",
            explanation = "Taal preserves its old Spanish-era buildings, traditions, and cultural heritage."
        },
        new Question
        {
            questionText = "Where can you buy Taal's famous delicacies?",
            optionA = "Only in Manila",
            optionB = "At the Taal Market and local shops",
            optionC = "Only online",
            correctAnswer = "B",
            explanation = "You can buy local delicacies like panutsa and taro products at Taal Market and shops around town."
        },
        new Question
        {
            questionText = "What is special about visiting Taal Heritage Town?",
            optionA = "You can see modern buildings only",
            optionB = "You can experience Philippine history and culture",
            optionC = "You can only go shopping",
            correctAnswer = "B",
            explanation = "Visiting Taal lets you experience preserved Spanish-era architecture, learn about Filipino heroes, and enjoy traditional culture."
        }
    };

    public List<Question> GetRandomQuestions(int count)
    {
        List<Question> shuffled = new List<Question>(allQuestions);
        
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Question temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }
        
        return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count));
    }
}