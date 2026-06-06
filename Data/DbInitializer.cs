using AcademicAIAssistant.Models;
using AcademicAIAssistant.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcademicAIAssistant.Data;

public static class DbInitializer
{
    public const string DemoEmail = "demo@student.com";
    public const string DemoPassword = "Demo@123456";
    private const string DemoDocumentFileName = "academic_ai_demo_paper.pdf";
    private const string DemoEssayTitle = "AI Tools and Academic Writing";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        bool enabled = configuration.GetValue("SeedData:Enabled", true);

        if (!enabled)
        {
            return;
        }

        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var referenceGenerator = serviceProvider.GetService<ReferenceGeneratorService>() ?? new ReferenceGeneratorService();

        var demoUser = await SeedDemoUserAsync(context);
        await SeedUserAISettingAsync(context, demoUser.Id);

        var document = await SeedDocumentAsync(context, demoUser.Id);
        var essay = await SeedEssayAsync(context, demoUser.Id);

        await SeedFeedbackReportAsync(context, essay.Id);
        await SeedCitationCheckAsync(context, essay.Id);
        await SeedSimilarityCheckAsync(context, essay.Id, document.Id);
        await SeedDocumentChatAsync(context, demoUser.Id, document.Id);
        await SeedTextScanAsync(context, demoUser.Id, document.Id);
        await SeedOcrScanAsync(context, demoUser.Id);
        await SeedWritingCoachSessionsAsync(context, demoUser.Id);
        await SeedReferencesAsync(context, demoUser.Id, referenceGenerator);
        await SeedKnowledgeGraphAsync(context, demoUser.Id, document.Id, essay.Id);
    }

    public static async Task ResetDemoDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        await DeleteDemoDataAsync(context);
        await SeedAsync(serviceProvider);
    }

    public static async Task DeleteDemoDataAsync(AppDbContext context)
    {
        var demoUser = await context.Users.FirstOrDefaultAsync(user => user.Email == DemoEmail);
        if (demoUser == null)
        {
            return;
        }

        int userId = demoUser.Id;
        var documentIds = await context.Documents
            .Where(document => document.UserId == userId)
            .Select(document => document.Id)
            .ToListAsync();
        var essayIds = await context.Essays
            .Where(essay => essay.UserId == userId)
            .Select(essay => essay.Id)
            .ToListAsync();
        var similarityCheckIds = await context.SimilarityChecks
            .Where(check => essayIds.Contains(check.EssayId))
            .Select(check => check.Id)
            .ToListAsync();
        var textScanIds = await context.TextScans
            .Where(scan => scan.UserId == userId)
            .Select(scan => scan.Id)
            .ToListAsync();

        context.GraphEdges.RemoveRange(context.GraphEdges.Where(edge => edge.UserId == userId));
        context.GraphNodes.RemoveRange(context.GraphNodes.Where(node => node.UserId == userId));

        context.DocumentChatMessages.RemoveRange(context.DocumentChatMessages.Where(message => message.UserId == userId));
        context.SimilarityMatches.RemoveRange(context.SimilarityMatches.Where(match =>
            similarityCheckIds.Contains(match.SimilarityCheckId) || documentIds.Contains(match.DocumentId)));
        context.SimilarityChecks.RemoveRange(context.SimilarityChecks.Where(check => essayIds.Contains(check.EssayId)));

        context.TextScanMatches.RemoveRange(context.TextScanMatches.Where(match => textScanIds.Contains(match.TextScanId)));
        context.TextScans.RemoveRange(context.TextScans.Where(scan => scan.UserId == userId));

        context.FeedbackReports.RemoveRange(context.FeedbackReports.Where(report => essayIds.Contains(report.EssayId)));
        context.CitationChecks.RemoveRange(context.CitationChecks.Where(check => essayIds.Contains(check.EssayId)));
        context.Essays.RemoveRange(context.Essays.Where(essay => essay.UserId == userId));

        context.Documents.RemoveRange(context.Documents.Where(document => document.UserId == userId));
        context.OCRScans.RemoveRange(context.OCRScans.Where(scan => scan.UserId == userId));
        context.WritingCoachSessions.RemoveRange(context.WritingCoachSessions.Where(session => session.UserId == userId));
        context.ReferenceItems.RemoveRange(context.ReferenceItems.Where(reference => reference.UserId == userId));
        context.UserAISettings.RemoveRange(context.UserAISettings.Where(setting => setting.UserId == userId));

        demoUser.FullName = "Demo Student";
        demoUser.Role = "Student";
        var passwordHasher = new PasswordHasher<User>();
        demoUser.PasswordHash = passwordHasher.HashPassword(demoUser, DemoPassword);

        await context.SaveChangesAsync();
    }

    private static async Task<User> SeedDemoUserAsync(AppDbContext context)
    {
        var user = await context.Users.FirstOrDefaultAsync(item => item.Email == DemoEmail);
        if (user != null)
        {
            return user;
        }

        user = new User
        {
            FullName = "Demo Student",
            Email = DemoEmail,
            Role = "Student",
            CreatedAt = DateTime.Now.AddDays(-5)
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, DemoPassword);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    private static async Task SeedUserAISettingAsync(AppDbContext context, int userId)
    {
        bool exists = await context.UserAISettings.AnyAsync(setting => setting.UserId == userId);
        if (exists)
        {
            return;
        }

        context.UserAISettings.Add(new UserAISetting
        {
            UserId = userId,
            Provider = "Gemini",
            ModelName = "gemini-2.5-flash",
            ApiKey = string.Empty,
            IsEnabled = false,
            CreatedAt = DateTime.Now.AddDays(-5)
        });

        await context.SaveChangesAsync();
    }

    private static async Task<Document> SeedDocumentAsync(AppDbContext context, int userId)
    {
        var document = await context.Documents
            .FirstOrDefaultAsync(item => item.UserId == userId && item.OriginalFileName == DemoDocumentFileName);

        if (document != null)
        {
            return document;
        }

        string extractedText = BuildDemoDocumentText();

        document = new Document
        {
            UserId = userId,
            Title = "Academic AI Demo Paper",
            OriginalFileName = DemoDocumentFileName,
            StoredFileName = "demo-academic-ai-paper.pdf",
            FilePath = "/uploads/documents/demo-academic-ai-paper.pdf",
            FileSize = 524_288,
            ContentType = "application/pdf",
            PageCount = 5,
            WordCount = CountWords(extractedText),
            UploadedAt = DateTime.Now.AddDays(-3),
            ExtractedAt = DateTime.Now.AddDays(-3),
            SummaryGeneratedAt = DateTime.Now.AddDays(-2),
            ExtractedText = extractedText,
            Summary = BuildDemoSummary()
        };

        context.Documents.Add(document);
        await context.SaveChangesAsync();

        return document;
    }

    private static async Task<Essay> SeedEssayAsync(AppDbContext context, int userId)
    {
        var essay = await context.Essays
            .FirstOrDefaultAsync(item => item.UserId == userId && item.Title == DemoEssayTitle);

        if (essay != null)
        {
            return essay;
        }

        string content = """
        AI tools are becoming part of academic writing because they can help students read research papers, organize ideas, and revise early drafts. This essay argues that AI tools can support academic writing when students use them critically and transparently. For example, AI assistants can summarize long papers and help students identify research problems more quickly (Nguyen, 2023). They can also provide feedback on grammar, structure, thesis clarity, and citation consistency.

        However, students should not treat AI output as a replacement for their own thinking. Academic writing still requires careful reading, evidence evaluation, and responsible citation. When students rely too heavily on automated suggestions, they may produce writing that sounds fluent but lacks original analysis. Therefore, universities should teach students how to use AI tools as learning support rather than as shortcuts.

        AI-based reading support systems may also improve access to difficult research texts. Tools such as summary generation, document chat, OCR scanning, and knowledge graph visualization can help students connect papers, keywords, and essay arguments (Tran & Le, 2024). In conclusion, AI tools can improve academic writing quality, but citation, originality, and critical thinking remain essential.

        References
        Nguyen, A. (2023). AI tools for academic writing. Journal of Educational Technology.
        Tran, B., & Le, C. (2024). Research reading support systems for university students. International Journal of Learning Innovation.
        """;

        essay = new Essay
        {
            UserId = userId,
            Title = DemoEssayTitle,
            EssayType = "Essay",
            Content = content,
            WordCount = CountWords(content),
            CreatedAt = DateTime.Now.AddDays(-2),
            UpdatedAt = DateTime.Now.AddDays(-1)
        };

        context.Essays.Add(essay);
        await context.SaveChangesAsync();

        return essay;
    }

    private static async Task SeedFeedbackReportAsync(AppDbContext context, int essayId)
    {
        bool exists = await context.FeedbackReports.AnyAsync(report => report.EssayId == essayId);
        if (exists)
        {
            return;
        }

        context.FeedbackReports.Add(new FeedbackReport
        {
            EssayId = essayId,
            OverallScore = 82,
            GrammarFeedback = "The essay is generally clear, but some sentences can be more concise.",
            AcademicToneFeedback = "The tone is appropriate for an academic essay.",
            ThesisFeedback = "The thesis statement is relevant but can be made more specific.",
            StructureFeedback = "The essay has a clear introduction, body, and conclusion.",
            LogicFeedback = "The argument flow is acceptable, but transitions can be improved.",
            CitationFeedback = "APA-style in-text citations and references are present.",
            GeneralSuggestions = "Add more evidence and explain how AI tools should be used responsibly.",
            CreatedAt = DateTime.Now.AddDays(-1)
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedCitationCheckAsync(AppDbContext context, int essayId)
    {
        bool exists = await context.CitationChecks.AnyAsync(check => check.EssayId == essayId);
        if (exists)
        {
            return;
        }

        context.CitationChecks.Add(new CitationCheck
        {
            EssayId = essayId,
            TotalInTextCitations = 2,
            TotalReferences = 2,
            MissingReferences = string.Empty,
            UnusedReferences = string.Empty,
            FormatIssues = string.Empty,
            OverallStatus = "Passed",
            CheckedAt = DateTime.Now.AddDays(-1)
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedSimilarityCheckAsync(AppDbContext context, int essayId, int documentId)
    {
        bool exists = await context.SimilarityChecks.AnyAsync(check => check.EssayId == essayId);
        if (exists)
        {
            return;
        }

        var check = new SimilarityCheck
        {
            EssayId = essayId,
            OverallSimilarityScore = 68,
            TotalDocumentsCompared = 1,
            TotalMatches = 1,
            Status = "High",
            CheckedAt = DateTime.Now.AddDays(-1)
        };

        check.Matches.Add(new SimilarityMatch
        {
            DocumentId = documentId,
            EssaySegment = "AI assistants can summarize long papers and help students identify research problems more quickly.",
            MatchedText = "AI academic assistants help students summarize research papers, identify important concepts, and connect reading with writing tasks.",
            SimilarityScore = 68,
            PageNumber = 2,
            CreatedAt = DateTime.Now.AddDays(-1)
        });

        context.SimilarityChecks.Add(check);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDocumentChatAsync(AppDbContext context, int userId, int documentId)
    {
        bool exists = await context.DocumentChatMessages.AnyAsync(message => message.UserId == userId && message.DocumentId == documentId);
        if (exists)
        {
            return;
        }

        context.DocumentChatMessages.Add(new DocumentChatMessage
        {
            UserId = userId,
            DocumentId = documentId,
            Question = "What is the main purpose of this paper?",
            Answer = "Based on the uploaded document, the paper explores how an Academic AI Assistant can support research reading and academic writing through summary generation, feedback, citation checking, similarity detection, OCR, and knowledge graph visualization.",
            SourceSnippet = "The purpose of the Academic AI Assistant is to support students in research reading and academic writing by combining document processing, writing feedback, citation checking, similarity detection, OCR scanning, and knowledge graph visualization.",
            CreatedAt = DateTime.Now.AddHours(-18)
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedTextScanAsync(AppDbContext context, int userId, int documentId)
    {
        bool exists = await context.TextScans.AnyAsync(scan => scan.UserId == userId && scan.Title == "Demo Similarity Scan");
        if (exists)
        {
            return;
        }

        var scan = new TextScan
        {
            UserId = userId,
            Title = "Demo Similarity Scan",
            InputText = "AI academic assistants help students summarize research papers, improve thesis clarity, check citations, and detect internal similarity across uploaded learning materials.",
            WordCount = 18,
            OverallSimilarityScore = 68,
            RiskLevel = "High",
            CreatedAt = DateTime.Now.AddDays(-1)
        };

        scan.Matches.Add(new TextScanMatch
        {
            SourceType = "Document",
            SourceId = documentId,
            SourceTitle = DemoDocumentFileName,
            InputSegment = "AI academic assistants help students summarize research papers, improve thesis clarity, check citations, and detect internal similarity.",
            MatchedSegment = "AI academic assistants help students summarize research papers, identify important concepts, and connect reading with writing tasks.",
            SimilarityScore = 68,
            CreatedAt = DateTime.Now.AddDays(-1)
        });

        context.TextScans.Add(scan);
        await context.SaveChangesAsync();
    }

    private static async Task SeedOcrScanAsync(AppDbContext context, int userId)
    {
        bool exists = await context.OCRScans.AnyAsync(scan => scan.UserId == userId && scan.Title == "Demo OCR Scan");
        if (exists)
        {
            return;
        }

        context.OCRScans.Add(new OCRScan
        {
            UserId = userId,
            Title = "Demo OCR Scan",
            OriginalFileName = "demo_ocr_note.png",
            StoredFileName = "demo_ocr_note.png",
            FilePath = "/uploads/ocr/demo_ocr_note.png",
            ContentType = "image/png",
            FileSize = 98_304,
            Language = "eng",
            ExtractedText = "AI writing assistants can help students brainstorm ideas, improve thesis statements, and organize academic essays.",
            CreatedAt = DateTime.Now.AddDays(-2)
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedWritingCoachSessionsAsync(AppDbContext context, int userId)
    {
        bool exists = await context.WritingCoachSessions.AnyAsync(session => session.UserId == userId);
        if (exists)
        {
            return;
        }

        context.WritingCoachSessions.AddRange(
            new WritingCoachSession
            {
                UserId = userId,
                Mode = "Brainstorm",
                Topic = "The impact of AI tools on academic writing",
                EssayType = "Essay",
                UserInput = "Generate ideas for an essay about AI tools and academic writing.",
                AIResponse = "Possible angles: writing support, research reading, citation responsibility, originality, and digital literacy. Research questions can focus on how students use AI tools responsibly.",
                Language = "en",
                CreatedAt = DateTime.Now.AddDays(-2)
            },
            new WritingCoachSession
            {
                UserId = userId,
                Mode = "Outline",
                Topic = "AI tools and university students",
                EssayType = "Essay",
                ThesisStatement = "AI tools can improve academic writing, but students need critical thinking to use them responsibly.",
                UserInput = "Create an outline for an essay about AI tools and university students.",
                AIResponse = "Introduction: define the problem. Body 1: research reading support. Body 2: writing feedback. Body 3: citation and originality. Counter-argument: overreliance on AI. Rebuttal: guided use. Conclusion: responsible AI literacy.",
                Language = "en",
                CreatedAt = DateTime.Now.AddDays(-1)
            },
            new WritingCoachSession
            {
                UserId = userId,
                Mode = "ThesisImprover",
                Topic = "Responsible AI use",
                EssayType = "Essay",
                ThesisStatement = "AI tools help students write better.",
                UserInput = "Improve this thesis statement.",
                AIResponse = "Improved thesis: AI tools can improve academic writing by supporting research reading, revision, and citation checking, but students must use them critically to protect originality and learning.",
                Language = "en",
                CreatedAt = DateTime.Now.AddHours(-20)
            });

        await context.SaveChangesAsync();
    }

    private static async Task SeedReferencesAsync(AppDbContext context, int userId, ReferenceGeneratorService referenceGenerator)
    {
        bool exists = await context.ReferenceItems.AnyAsync(reference => reference.UserId == userId);
        if (exists)
        {
            return;
        }

        var references = new List<ReferenceItem>
        {
            new()
            {
                UserId = userId,
                SourceType = "Book",
                Author = "Nguyen, A.",
                Year = "2023",
                Title = "Academic Writing with AI Support",
                JournalOrPublisher = "Education Press",
                Notes = "Demo book reference for Academic AI Assistant.",
                CreatedAt = DateTime.Now.AddDays(-2)
            },
            new()
            {
                UserId = userId,
                SourceType = "JournalArticle",
                Author = "Tran, B., & Le, C.",
                Year = "2024",
                Title = "Research Reading Support Systems for University Students",
                JournalOrPublisher = "International Journal of Learning Innovation",
                Volume = "12",
                Issue = "2",
                Pages = "45-60",
                Doi = "10.1234/demo.2024.001",
                Notes = "Demo journal article reference.",
                CreatedAt = DateTime.Now.AddDays(-2)
            },
            new()
            {
                UserId = userId,
                SourceType = "Website",
                Author = "Open Learning Lab",
                Year = "2025",
                Title = "Responsible Use of AI in Academic Writing",
                WebsiteName = "Open Learning Lab",
                Url = "https://example.com/responsible-ai-writing",
                AccessDate = DateTime.Today,
                Notes = "Demo website reference.",
                CreatedAt = DateTime.Now.AddDays(-1)
            }
        };

        foreach (var reference in references)
        {
            referenceGenerator.GenerateAllCitationFormats(reference);
        }

        context.ReferenceItems.AddRange(references);
        await context.SaveChangesAsync();
    }

    private static async Task SeedKnowledgeGraphAsync(AppDbContext context, int userId, int documentId, int essayId)
    {
        bool exists = await context.GraphNodes.AnyAsync(node => node.UserId == userId);
        if (exists)
        {
            return;
        }

        var documentNode = new GraphNode
        {
            UserId = userId,
            NodeType = "Document",
            Label = DemoDocumentFileName,
            SourceType = "Document",
            SourceId = documentId,
            Metadata = "Demo research PDF",
            CreatedAt = DateTime.Now.AddDays(-1)
        };

        var essayNode = new GraphNode
        {
            UserId = userId,
            NodeType = "Essay",
            Label = DemoEssayTitle,
            SourceType = "Essay",
            SourceId = essayId,
            Metadata = "Demo academic essay",
            CreatedAt = DateTime.Now.AddDays(-1)
        };

        var academicWritingNode = CreateKeywordNode(userId, "academic writing");
        var aiFeedbackNode = CreateKeywordNode(userId, "AI feedback");
        var citationCheckingNode = CreateKeywordNode(userId, "citation checking");

        context.GraphNodes.AddRange(documentNode, essayNode, academicWritingNode, aiFeedbackNode, citationCheckingNode);
        await context.SaveChangesAsync();

        context.GraphEdges.AddRange(
            CreateEdge(userId, documentNode.Id, academicWritingNode.Id, "HAS_KEYWORD", 1),
            CreateEdge(userId, documentNode.Id, aiFeedbackNode.Id, "HAS_KEYWORD", 1),
            CreateEdge(userId, documentNode.Id, citationCheckingNode.Id, "HAS_KEYWORD", 1),
            CreateEdge(userId, essayNode.Id, academicWritingNode.Id, "HAS_KEYWORD", 1),
            CreateEdge(userId, essayNode.Id, citationCheckingNode.Id, "HAS_KEYWORD", 1),
            CreateEdge(userId, essayNode.Id, documentNode.Id, "USED_IN_ESSAY", 2));

        await context.SaveChangesAsync();
    }

    private static GraphNode CreateKeywordNode(int userId, string label)
    {
        return new GraphNode
        {
            UserId = userId,
            NodeType = "Keyword",
            Label = label,
            SourceType = "Keyword",
            SourceId = null,
            CreatedAt = DateTime.Now.AddDays(-1)
        };
    }

    private static GraphEdge CreateEdge(int userId, int sourceNodeId, int targetNodeId, string relationType, double weight)
    {
        return new GraphEdge
        {
            UserId = userId,
            SourceNodeId = sourceNodeId,
            TargetNodeId = targetNodeId,
            RelationType = relationType,
            Weight = weight,
            CreatedAt = DateTime.Now.AddDays(-1)
        };
    }

    private static string BuildDemoDocumentText()
    {
        return """
        Abstract
        This paper presents an Academic AI Assistant designed to support university students in research reading and academic writing. The system combines PDF upload, text extraction, rule-based summary generation, writing feedback, citation checking, internal similarity detection, OCR scanning, document chat, and knowledge graph visualization. The goal is not to replace student thinking, but to provide a structured workspace for reading and writing tasks.

        Introduction
        Students often struggle with long research papers, unfamiliar academic vocabulary, and the pressure to write essays with clear structure and accurate citations. Existing tools may support grammar correction or document search, but many student workflows remain fragmented. An Academic AI Assistant can help students summarize research reading, organize extracted information, and connect ideas across documents and essays.

        Methodology
        The prototype was developed using ASP.NET Core MVC, Entity Framework Core, SQL Server LocalDB, Bootstrap, and rule-based service layers. PDF text is extracted and stored as document metadata. Writing feedback is generated through rules for word count, thesis statement, academic tone, paragraph structure, logic transitions, and APA citation patterns. The system also includes internal similarity detection by comparing essay segments with extracted document text. OCR scanning supports image-based notes and screenshots.

        Findings
        The prototype shows that a single academic workspace can reduce the number of separate tools students need during research reading and writing. Summary generation helps students preview papers quickly. Chat with PDF helps students ask targeted questions about methodology, findings, conclusion, and references. Citation checking and similarity detection encourage responsible academic writing. Knowledge graph visualization helps students see relationships among documents, essays, and keywords such as academic writing, research reading, AI feedback, citation checking, similarity detection, knowledge graph, OCR scanner, and writing coach.

        Conclusion
        Academic AI Assistant demonstrates a practical software engineering solution for student research support. The rule-based fallback keeps the application usable even when AI mode is disabled or an API key is unavailable. Future development can improve semantic retrieval, multilingual feedback, advanced citation validation, and integration with real AI models. The system should continue to emphasize responsible AI use, originality, citation accuracy, and critical thinking.

        Keywords
        academic writing, research reading, AI feedback, citation checking, similarity detection, knowledge graph, OCR scanner, writing coach
        """;
    }

    private static string BuildDemoSummary()
    {
        return """
        Short Summary:
        The demo paper describes an Academic AI Assistant that supports research reading and academic writing through PDF processing, writing feedback, citation checking, similarity detection, OCR, chat, and knowledge graph visualization.

        Research Problem:
        Students often need multiple disconnected tools to read research papers, manage citations, and improve academic essays.

        Methodology:
        The prototype uses ASP.NET Core MVC, Entity Framework Core, SQL Server LocalDB, Bootstrap, and rule-based services with optional AI fallback.

        Main Findings:
        A single academic workspace can help students preview papers, ask questions about documents, improve essay structure, check citations, and identify internal similarity.

        Limitations:
        Rule-based logic is simplified and scanned PDF support depends on OCR quality.

        Keywords:
        academic writing, research reading, AI feedback, citation checking, similarity detection, knowledge graph, OCR scanner, writing coach
        """;
    }

    private static int CountWords(string text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
