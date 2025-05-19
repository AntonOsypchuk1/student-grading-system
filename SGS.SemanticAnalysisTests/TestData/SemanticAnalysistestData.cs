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