# Features

| Module | Feature | Description | User Role | Main Route | Current Status |
|---|---|---|---|---|---|
| Authentication | Register | Create a student account with full name, email, and password | Student | `/Account/Register` | Completed |
| Authentication | Login | Sign in using email and password | Student | `/Account/Login` | Completed |
| Authentication | Logout | Clear session/cookie and sign out | Student | `/Account/Logout` | Completed |
| Dashboard | Overview | Show statistics and recent activities for current user | Student | `/Dashboard` | Completed |
| Document Library | List documents | Show uploaded PDF documents owned by current user | Student | `/Documents` | Completed |
| PDF Upload | Upload PDF | Upload PDF file and save metadata | Student | `/Documents/Upload` | Completed |
| PDF Text Extraction | Extract text | Extract text, page count, and word count from PDF | Student | `/Documents/ExtractText/{id}` | Completed |
| Summary Generation | Generate summary | Generate summary from extracted document text | Student | `/Documents/GenerateSummary/{id}` | Rule-based / AI-ready |
| Document Details | View document | Show metadata, summary, text preview, and actions | Student | `/Documents/Details/{id}` | Completed |
| Writing Studio | Analyze essay | Submit essay/literature review/thesis proposal for feedback | Student | `/Writing` | Completed |
| Writing Feedback | Feedback report | Show score and feedback for grammar, tone, thesis, structure, logic, citation | Student | `/Writing/Details/{id}` | Rule-based / AI-ready |
| Writing History | Essay list | Show previous analyzed essays | Student | `/Writing/History` | Completed |
| Citation Checker | Check citation | Check basic APA citations and References section | Student | `/Citation/Check/{essayId}` | Rule-based |
| Citation Checker | Result | Show missing references, unused references, format issues, status | Student | `/Citation/Result/{id}` | Completed |
| Similarity Checker | Check similarity | Compare essay with current user's extracted documents | Student | `/Similarity/Check/{essayId}` | Rule-based |
| Similarity Checker | Result | Show score, status, and matched segments | Student | `/Similarity/Result/{id}` | Completed |
| Knowledge Graph | Build graph | Build document, essay, keyword nodes and relationships | Student | `/Graph/Build` | Rule-based |
| Knowledge Graph | View graph | Render graph using frontend JavaScript | Student | `/Graph` | Completed |
| Knowledge Graph | Graph data | Return current user's graph nodes and edges as JSON | Student | `/Graph/Data` | Completed |
| Chat with PDF | Ask question | Ask a question about an extracted PDF | Student | `/DocumentChat/{documentId}` | Rule-based / AI-ready |
| Chat with PDF | Chat history | Save question, answer, source snippet, and time | Student | `/DocumentChat/{documentId}` | Completed |
| AIService | AI fallback structure | Optional OpenAI-compatible service with rule-based fallback | Student | Service layer | AI-ready |
