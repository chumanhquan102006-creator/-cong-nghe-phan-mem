# Project Summary

## Project Overview

Academic AI Assistant is a web-based academic reading and writing support system for students. It combines document management, PDF text extraction, writing feedback, citation checking, internal similarity checking, knowledge graph visualization, and document chat in one ASP.NET Core MVC application.

## Target Users

- University students
- Students writing essays, literature reviews, research reports, or thesis proposals
- Student groups who need to manage research papers and academic drafts

## Main Problem Solved

Students often struggle with reading research PDFs, summarizing papers, organizing ideas, checking academic writing quality, managing citations, and connecting sources to their essays. This project provides one integrated workflow to support those tasks.

## Existing Systems Referenced

- Grammarly: writing feedback, grammar, tone, and clarity support
- SciSpace: research paper reading and document Q&A
- Consensus: research-oriented discovery and evidence support

## Proposed Solution

The proposed system gives students a local academic workspace:

- Upload research PDFs.
- Extract text from PDFs.
- Generate summaries.
- Ask questions about uploaded documents.
- Write and analyze essays.
- Check citation format and references.
- Compare essays with uploaded extracted documents.
- Build a knowledge graph from documents, essays, and keywords.

## Main Modules

- Authentication
- Dashboard
- Document Library
- PDF Upload
- PDF Text Extraction
- Summary Generation
- Writing Studio
- Writing Feedback
- Citation Checker
- Similarity Checker
- Knowledge Graph
- Chat with PDF
- AIService fallback structure

## Database Overview

Main database entities:

- `User`: account information and role
- `Document`: uploaded PDF metadata, extracted text, summary, word count, page count
- `Essay`: academic writing content submitted by user
- `FeedbackReport`: writing analysis result
- `CitationCheck`: citation checking result
- `SimilarityCheck`: overall similarity checking result
- `SimilarityMatch`: matched text segment between essay and document
- `GraphNode`: document, essay, and keyword nodes
- `GraphEdge`: relationships between graph nodes
- `DocumentChatMessage`: saved chat question, answer, and source snippet

## AI and Rule-Based Approach

The system is designed to run even without a real AI API key.

- Rule-based services provide mock/demo behavior for summary, writing feedback, citation checking, similarity checking, keyword extraction, and document chat.
- `AIService` is separated from controllers and can call an OpenAI-compatible API when enabled.
- If AI is disabled, missing API key, timeout, or API error occurs, the app falls back to rule-based services.

## Limitations

- PDF extraction may not work well with scanned/image-only PDFs.
- Rule-based writing feedback is simplified and not as accurate as a real AI writing assistant.
- Citation checking supports only basic APA-style patterns.
- Similarity checking only compares against documents uploaded and extracted by the same user.
- Knowledge Graph uses keyword frequency, not semantic understanding.
- Chat with PDF uses simple retrieval and fallback answer generation.

## Future Improvements

- Add OCR for scanned PDFs.
- Improve AI prompts and structured AI response parsing.
- Add full citation style support for APA, MLA, IEEE.
- Add export to PDF/Word for feedback reports.
- Add admin dashboard.
- Add better graph filtering and search.
- Add vector database for more accurate document chat.
- Add automated unit and integration tests.
