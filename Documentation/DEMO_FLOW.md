# Demo Flow

Prepare before demo:

- One registered demo account.
- One text-based research PDF.
- One essay draft with at least one APA-style citation.
- AI mode can remain disabled because the app supports rule-based fallback.

## 1. Login

- Page/URL: `/Account/Login`
- Action: Enter demo email and password, then click **Login**.
- Expected result: User is redirected to Dashboard.
- What to say: "The system supports account-based access, so each student sees only their own workspace."

## 2. Dashboard Overview

- Page/URL: `/Dashboard`
- Action: Show statistics, quick actions, recent documents, recent essays, and recent activities.
- Expected result: Dashboard summarizes the user's academic reading and writing activity.
- What to say: "This is the central workspace for monitoring uploaded documents, essays, checks, graph data, and recent actions."

## 3. Upload PDF

- Page/URL: `/Documents/Upload`
- Action: Enter document title, select PDF, click **Upload**.
- Expected result: Success message appears and document is listed in Document Library.
- What to say: "Students can store research papers and keep metadata in the database."

## 4. Extract Text

- Page/URL: `/Documents/Details/{id}`
- Action: Click **Extract Text**.
- Expected result: Page count, word count, extracted time, and text preview appear.
- What to say: "Text extraction is the foundation for summary, chat, similarity, and graph features."

## 5. Generate Summary

- Page/URL: `/Documents/Details/{id}`
- Action: Click **Generate Summary**.
- Expected result: Summary appears in the Summary card.
- What to say: "The app can run with AI disabled by using a rule-based fallback summary."

## 6. Chat with PDF

- Page/URL: `/DocumentChat/{documentId}`
- Action: Ask `summarize this paper` or `what is the methodology?`.
- Expected result: Answer and source snippet are saved in chat history.
- What to say: "This is a mock/rule-based document Q&A workflow that can later be improved with real AI."

## 7. Writing Studio

- Page/URL: `/Writing`
- Action: Enter title, essay type, paste content, click **Analyze Essay**.
- Expected result: User is redirected to Writing Details.
- What to say: "Students can submit drafts such as essays, literature reviews, or thesis proposals."

## 8. Writing Feedback Result

- Page/URL: `/Writing/Details/{id}`
- Action: Show overall score and feedback cards.
- Expected result: Feedback appears for grammar, tone, thesis, structure, logic, citation, and suggestions.
- What to say: "Feedback is rule-based by default and AI-ready through AIService fallback."

## 9. Citation Checker

- Page/URL: `/Citation/Check/{essayId}`
- Action: Click **Check Citation**.
- Expected result: Citation result shows total citations, references, missing references, unused references, format issues, and status.
- What to say: "The checker validates basic APA-style in-text citations and the References section."

## 10. Similarity Checker

- Page/URL: `/Similarity/Check/{essayId}`
- Action: Click **Check Similarity**.
- Expected result: Result shows score, status, compared documents, and matched text.
- What to say: "This is internal similarity checking against the user's uploaded extracted PDFs, not an Internet plagiarism checker."

## 11. Knowledge Graph

- Page/URL: `/Graph`
- Action: Click **Build / Refresh Graph**.
- Expected result: Graph displays document, essay, and keyword nodes with relationships.
- What to say: "The graph visualizes how papers and essays connect through shared keywords."

## 12. Dashboard Recent Activities

- Page/URL: `/Dashboard`
- Action: Return to Dashboard.
- Expected result: Recent activities show the actions performed during the demo.
- What to say: "The dashboard updates as the student uses the system."

## 13. Logout

- Page/URL: Navbar Logout
- Action: Click **Logout**.
- Expected result: User is signed out and returned to Home.
- What to say: "The session is cleared, and protected pages require login again."
