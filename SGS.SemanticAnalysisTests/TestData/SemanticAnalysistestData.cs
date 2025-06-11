// using System.Text;
// using SGS.Domain.Entities;
// using SGS.SemanticAnalysis.Structures.PromptRequests;
//
// namespace SGS.SemanticAnalysisTests.TestData;
//
// /// <summary>
// /// Містить зразки документів та критеріїв для тестування функціональності семантичного аналізу
// /// </summary>
// public static class SemanticAnalysisTestData
// {
//     // Зразки документів
//     public static class Documents
//     {
//         public static string WellWrittenDocument => 
//             "Це добре написаний документ, що пояснює концепцію семантичного аналізу. " +
//             "Текст є чітким та лаконічним, з гарною структурою та організацією. " +
//             "Він охоплює базові принципи без зайвих деталей. " +
//             "Документ використовує відповідну термінологію та надає приклади для ілюстрації ідей.";
//
//         public static string PoorQualityDocument => 
//             "Цей документ містить багато помилок і не має чіткої структури. " +
//             "Є орфографічні помилки, і він зовсім не читається плавно. " +
//             "Інформація подана погано, і немає чіткого пояснення концепцій.";
//
//         public static string TechnicalApiDocumentation => 
//             "# Документація API\n\n" +
//             "## Ендпоінти\n\n" +
//             "### GET /api/users\n\n" +
//             "Повертає список користувачів.\n\n" +
//             "**Параметри:**\n" +
//             "- page (необов'язковий): Номер сторінки, за замовчуванням 1\n" +
//             "- limit (необов'язковий): Кількість результатів на сторінку, за замовчуванням 20\n\n" +
//             "**Відповідь:**\n" +
//             "```json\n" +
//             "{\n" +
//             "  \"users\": [\n" +
//             "    {\n" +
//             "      \"id\": 1,\n" +
//             "      \"name\": \"John Doe\",\n" +
//             "      \"email\": \"john@example.com\"\n" +
//             "    }\n" +
//             "  ],\n" +
//             "  \"total\": 100,\n" +
//             "  \"page\": 1,\n" +
//             "  \"limit\": 20\n" +
//             "}\n" +
//             "```";
//
//         public static string ArtificialIntelligenceOverview => 
//             "Цей документ надає огляд штучного інтелекту та його застосувань. " +
//             "Машинне навчання — це підмножина ШІ, що зосереджується на алгоритмах, які можуть навчатися на даних. " +
//             "Обробка природної мови — ще одна важлива сфера досліджень ШІ. " +
//             "Системи ШІ тепер можуть виконувати завдання, які раніше вимагали людського інтелекту.";
//
//         // Ви можете додати додаткові документи для різних сценаріїв тестування
//         public static string LongDocument => 
//             "# Комплексний звіт про зміну клімату\n\n" +
//             "## Виконавче резюме\n\n" +
//             "Зміна клімату є однією з найважливіших проблем нашого часу. Цей звіт надає детальний " +
//             "аналіз поточних тенденцій, наслідків та можливих стратегій пом'якшення. Наші висновки вказують, " +
//             "що необхідні негайні дії у кількох секторах для вирішення цієї глобальної проблеми.\n\n" +
//             "## Вступ\n\n" +
//             "Клімат Землі змінювався протягом історії. Лише за останні 650 000 років було сім " +
//             "циклів просування та відступу льодовиків. Поточний тренд потепління має особливе значення, " +
//             "оскільки є наслідком діяльності людини з середини XX століття та відбувається з безпрецедентною швидкістю.\n\n" +
//             "## Поточні спостереження\n\n" +
//             "Підвищення глобальної температури: середня поверхнева температура планети зросла приблизно " +
//             "на 2,12 градуси за Фаренгейтом (1,18 градуса за Цельсієм) з кінця XIX століття, що здебільшого " +
//             "спричинено збільшенням викидів вуглецю.\n\n" +
//             "Потепління океанів: океани поглинули більшу частину цього збільшеного тепла, причому перші " +
//             "100 метрів показують потепління більш ніж на 0,33 градуса за Цельсієм з 1969 року.\n\n" +
//             "Скорочення льодовикових покривів: льодовикові шапки Гренландії та Антарктики зменшилися " +
//             "в масі. Гренландія втрачала в середньому 286 мільярдів тонн льоду на рік між 1993 та 2016 роками.\n\n" +
//             "## Стратегії пом'якшення\n\n" +
//             "Зменшення викидів вуглецю через політичні зміни та технологічні інновації є нашим " +
//             "найкращим шляхом вперед. Це включає перехід на відновлювані джерела енергії, покращення енергоефективності " +
//             "та розробку технологій уловлювання вуглецю.\n\n" +
//             "## Висновок\n\n" +
//             "Наукові докази потепління кліматичної системи є беззаперечними. Негайні та скоординовані дії " +
//             "необхідні для пом'якшення найгірших наслідків зміни клімату для майбутніх поколінь.";
//     }
//
//     // Зразкові критерії оцінювання
//     public static class Criteria
//     {
//         public static List<CriterionInfo> BasicDocumentCriteria =>
//         [
//             new()
//             {
//                 Id = Guid.NewGuid().ToString(),
//                 Title = "Ясність",
//                 Description = "Документ має бути легким для розуміння та використовувати чітку мову",
//                 MaxGrade = 100,
//                 Weight = 0.6f
//             },
//
//             new()
//             {
//                 Id = Guid.NewGuid().ToString(),
//                 Title = "Структура",
//                 Description = "Документ має бути добре організований із логічним потоком",
//                 MaxGrade = 100,
//                 Weight = 0.4f
//             }
//         ];
//
//         public static List<CriterionInfo> GrammarAndSpellingCriteria =>
//         [
//             new()
//             {
//                 Id = Guid.NewGuid().ToString(),
//                 Title = "Ясність",
//                 Description = "Документ має бути легким для розуміння та використовувати чітку мову",
//                 MaxGrade = 100,
//                 Weight = 0.5f
//             },
//
//             new()
//             {
//                 Id = Guid.NewGuid().ToString(),
//                 Title = "Граматика та правопис",
//                 Description = "Документ має бути вільним від граматичних і орфографічних помилок",
//                 MaxGrade = 100,
//                 Weight = 0.5f
//             }
//         ];
//
//         public static List<CriterionInfo> TechnicalDocumentationCriteria =>
//         [
//             new()
//             {
//                 Id = Guid.NewGuid().ToString(),
//                 Title = "Повнота документації API",
//                 Description =
//                     "Документація містить усю необхідну інформацію для кінцевих точок API, включаючи параметри, відповіді та приклади",
//                 MaxGrade = 100,
//                 Weight = 0.5f
//             },
//
//             new()
//             {
//                 Id = Guid.NewGuid().ToString(),
//                 Title = "Технічна точність",
//                 Description = "Інформація є технічно точною та відповідає галузевим стандартам",
//                 MaxGrade = 100,
//                 Weight = 0.5f
//             }
//         ];
//
//         public static List<GradingCriteria> FullAnalysisCriteria =>
//         [
//             new()
//             {
//                 Id = Guid.NewGuid(),
//                 Title = "Точність змісту",
//                 Description = "Надана інформація є точною та актуальною",
//                 MaxGrade = 100,
//                 Weight = 0.4f
//             },
//
//             new()
//             {
//                 Id = Guid.NewGuid(),
//                 Title = "Повнота",
//                 Description = "Включена вся необхідна інформація",
//                 MaxGrade = 100,
//                 Weight = 0.3f
//             },
//
//             new()
//             {
//                 Id = Guid.NewGuid(),
//                 Title = "Ясність",
//                 Description = "Інформація подана чітко",
//                 MaxGrade = 100,
//                 Weight = 0.3f
//             }
//         ];
//     }
//
//     // Зразкові результати оцінювання
//     public static class EvaluationResults
//     {
//         public static string SampleEvaluationDetails => new StringBuilder()
//             .AppendLine("## Ясність (Бал: 85.0/100, Вага: 0.60)")
//             .AppendLine("**Відгук**: Документ написано чіткою мовою, що легко сприймається.")
//             .AppendLine("**Сильні сторони**:")
//             .AppendLine("- Використовує лаконічні та прості речення")
//             .AppendLine("- По можливості уникає технічного жаргону")
//             .AppendLine("**Напрями для покращення**:")
//             .AppendLine("- Можна додати більше прикладів у деяких розділах")
//             .AppendLine()
//             .AppendLine("## Структура (Бал: 78.0/100, Вага: 0.40)")
//             .AppendLine("**Відгук**: Загальна структура хороша, але деякі розділи можуть бути побудовані краще.")
//             .AppendLine("**Сильні сторони**:")
//             .AppendLine("- Чіткі заголовки розділів")
//             .AppendLine("- Логічний розвиток тем")
//             .AppendLine("**Напрями для покращення**:")
//             .AppendLine("- Більш рівномірна довжина абзаців")
//             .AppendLine("- Можливо покращити переходи між розділами")
//             .ToString();
//     }
// }


using System.Text;
using SGS.Domain.Entities;
using SGS.SemanticAnalysis.Structures.PromptRequests;

namespace SGS.SemanticAnalysisTests.TestData;

/// <summary>
/// Contains sample documents and criteria for testing the semantic analysis functionality
/// </summary>
public static class SemanticAnalysisTestData
{
    // Sample Documents
    public static class Documents
    {
        public static string WellWrittenDocument => 
            "This is a well-written document explaining the concept of semantic analysis. " +
            "The text is clear and concise, with good structure and organization. " +
            "It covers the basic principles without unnecessary details. " +
            "The document uses appropriate terminology and provides examples to illustrate points.";

        public static string PoorQualityDocument => 
            "This documint has many errors it is not structured. " +
            "there are spelling mistakesto be found and it doesn't flow well at all. " +
            "the information is presented poorly and there is no clear explanation of the concepts.";

        public static string TechnicalApiDocumentation => 
            "# API Documentation\n\n" +
            "## Endpoints\n\n" +
            "### GET /api/users\n\n" +
            "Returns a list of users.\n\n" +
            "**Parameters:**\n" +
            "- page (optional): Page number, default is 1\n" +
            "- limit (optional): Number of results per page, default is 20\n\n" +
            "**Response:**\n" +
            "```json\n" +
            "{\n" +
            "  \"users\": [\n" +
            "    {\n" +
            "      \"id\": 1,\n" +
            "      \"name\": \"John Doe\",\n" +
            "      \"email\": \"john@example.com\"\n" +
            "    }\n" +
            "  ],\n" +
            "  \"total\": 100,\n" +
            "  \"page\": 1,\n" +
            "  \"limit\": 20\n" +
            "}\n" +
            "```";

        public static string ArtificialIntelligenceOverview => 
            "This document provides an overview of artificial intelligence and its applications. " +
            "Machine learning is a subset of AI that focuses on algorithms that can learn from data. " +
            "Natural language processing is another important area of AI research. " +
            "AI systems can now perform tasks that previously required human intelligence.";

        // You could add additional documents for different testing scenarios
        public static string LongDocument => 
            "# Comprehensive Report on Climate Change\n\n" +
            "## Executive Summary\n\n" +
            "Climate change represents one of the most significant challenges of our time. This report provides a detailed " +
            "analysis of current trends, impacts, and potential mitigation strategies. Our findings indicate that immediate " +
            "action is required across multiple sectors to address this global issue.\n\n" +
            "## Introduction\n\n" +
            "The Earth's climate has changed throughout history. Just in the last 650,000 years, there have been seven " +
            "cycles of glacial advance and retreat. The current warming trend is of particular significance because it is " +
            "primarily the result of human activity since the mid-20th century and is proceeding at an unprecedented rate.\n\n" +
            "## Current Observations\n\n" +
            "Global temperature rise: The planet's average surface temperature has risen about 2.12 degrees Fahrenheit (1.18 " +
            "degrees Celsius) since the late 19th century, a change driven largely by increased carbon dioxide emissions.\n\n" +
            "Warming oceans: The oceans have absorbed much of this increased heat, with the top 100 meters of ocean showing " +
            "warming of more than 0.33 degrees Celsius since 1969.\n\n" +
            "Shrinking ice sheets: The Greenland and Antarctic ice sheets have decreased in mass. Greenland lost an average " +
            "of 286 billion tons of ice per year between 1993 and 2016.\n\n" +
            "## Mitigation Strategies\n\n" +
            "Reducing carbon emissions through policy changes and technological innovations represents our best path forward. " +
            "This includes transitioning to renewable energy sources, improving energy efficiency, and developing carbon " +
            "capture technologies.\n\n" +
            "## Conclusion\n\n" +
            "The scientific evidence for warming of the climate system is unequivocal. Immediate and coordinated action " +
            "is necessary to mitigate the worst effects of climate change for future generations.";
    }

    // Sample Evaluation Criteria
    public static class Criteria
    {
        public static List<CriterionInfo> BasicDocumentCriteria =>
        [
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Clarity",
                Description = "The document should be easy to understand and use clear language",
                MaxGrade = 100,
                Weight = 0.6f
            },

            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Structure",
                Description = "The document should be well-organized with a logical flow",
                MaxGrade = 100,
                Weight = 0.4f
            }
        ];

        public static List<CriterionInfo> GrammarAndSpellingCriteria =>
        [
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Clarity",
                Description = "The document should be easy to understand and use clear language",
                MaxGrade = 100,
                Weight = 0.5f
            },

            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Grammar & Spelling",
                Description = "The document should be free of grammatical and spelling errors",
                MaxGrade = 100,
                Weight = 0.5f
            }
        ];

        public static List<CriterionInfo> TechnicalDocumentationCriteria =>
        [
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "API Documentation Completeness",
                Description =
                    "Documentation includes all necessary information for API endpoints including parameters, responses, and examples",
                MaxGrade = 100,
                Weight = 0.5f
            },

            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Technical Accuracy",
                Description = "Information is technically accurate and follows industry standards",
                MaxGrade = 100,
                Weight = 0.5f
            }
        ];

        public static List<GradingCriteria> FullAnalysisCriteria =>
        [
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Content Accuracy",
                Description = "Information provided is accurate and up-to-date",
                MaxGrade = 100,
                Weight = 0.4f
            },

            new()
            {
                Id = Guid.NewGuid(),
                Title = "Completeness",
                Description = "All necessary information is included",
                MaxGrade = 100,
                Weight = 0.3f
            },

            new()
            {
                Id = Guid.NewGuid(),
                Title = "Clarity",
                Description = "Information is presented clearly",
                MaxGrade = 100,
                Weight = 0.3f
            }
        ];
    }

    // Sample Evaluation Results
    public static class EvaluationResults
    {
        public static string SampleEvaluationDetails => new StringBuilder()
            .AppendLine("## Clarity (Score: 85.0/100, Weight: 0.60)")
            .AppendLine("**Feedback**: The document is written in clear language that is easy to understand.")
            .AppendLine("**Strengths**:")
            .AppendLine("- Uses concise and straightforward sentences")
            .AppendLine("- Avoids technical jargon when possible")
            .AppendLine("**Areas for Improvement**:")
            .AppendLine("- Could provide more examples in some areas")
            .AppendLine()
            .AppendLine("## Structure (Score: 78.0/100, Weight: 0.40)")
            .AppendLine("**Feedback**: Good overall structure but some sections could flow better.")
            .AppendLine("**Strengths**:")
            .AppendLine("- Clear section headings")
            .AppendLine("- Logical progression of topics")
            .AppendLine("**Areas for Improvement**:")
            .AppendLine("- More consistent paragraph lengths")
            .AppendLine("- Could improve transitions between sections")
            .ToString();
    }
}