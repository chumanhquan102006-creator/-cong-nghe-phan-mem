namespace AcademicAIAssistant.Services;

public class WritingCoachFallbackService
{
    public string GenerateResponse(string mode, string topic, string essayType, string thesisStatement, string userInput, string language)
    {
        return language.Equals("vi", StringComparison.OrdinalIgnoreCase)
            ? GenerateVietnamese(mode, topic, essayType, thesisStatement, userInput)
            : GenerateEnglish(mode, topic, essayType, thesisStatement, userInput);
    }

    private static string GenerateEnglish(string mode, string topic, string essayType, string thesisStatement, string userInput)
    {
        string subject = FirstNonEmpty(topic, thesisStatement, userInput, "your topic");

        return mode switch
        {
            "Outline" => $"""
                Academic Outline for: {subject}

                Introduction:
                - Introduce the context and why the topic matters.
                - Narrow the focus to a specific academic problem.
                - Working thesis: {FirstNonEmpty(thesisStatement, "Develop a clear position about " + subject)}.

                Body Paragraph 1:
                - Main idea: Define the key concept or background.
                - Evidence needed: scholarly source, data, or example.
                - Explanation: connect the evidence to the thesis.

                Body Paragraph 2:
                - Main idea: Analyze the most important benefit, problem, or cause.
                - Evidence needed: research findings or case example.
                - Explanation: show why this point strengthens the argument.

                Body Paragraph 3:
                - Main idea: Discuss implication, application, or limitation.
                - Evidence needed: comparison, expert view, or study result.

                Counter-argument:
                - Present a reasonable opposing view.

                Rebuttal:
                - Explain why the thesis remains stronger or more balanced.

                Conclusion:
                - Restate the argument, summarize key points, and suggest future research.
                """,
            "CounterArgument" => $"""
                Counter-arguments for: {subject}

                1. Cost or feasibility concern:
                - Opposing view: the idea may be difficult to implement in real academic or institutional contexts.
                - Rebuttal: narrow the scope and explain realistic conditions where the argument still works.

                2. Ethical concern:
                - Opposing view: the argument may create fairness, privacy, or access issues.
                - Rebuttal: acknowledge the concern and propose safeguards or limits.

                3. Effectiveness limitation:
                - Opposing view: the argument may not work equally well for all groups or contexts.
                - Rebuttal: explain when it is effective and identify remaining limitations.

                Useful transition:
                - Although this concern is valid, the evidence suggests that...
                """,
            "ThesisImprover" => $"""
                Thesis Statement Feedback

                Current thesis:
                {FirstNonEmpty(thesisStatement, subject)}

                Suggestions:
                - Make the thesis more specific.
                - Add a clear position, not only a topic.
                - Add scope or context.
                - Use academic wording and avoid overly broad claims.

                Improved versions:
                1. Basic: This paper argues that {subject} has important academic implications that should be examined more carefully.
                2. Strong academic: This paper argues that {subject} should be understood through its impact on learning outcomes, access, and academic responsibility.
                3. Research-focused: This study examines how {subject} shapes student learning and identifies the conditions under which it is most effective.
                """,
            _ => $"""
                Brainstorm Ideas for: {subject}

                Possible angles:
                1. Educational impact
                2. Student experience
                3. Ethical concerns
                4. Technology and access
                5. Long-term academic outcomes

                Research questions:
                1. How does {subject} affect student learning?
                2. What benefits and limitations are associated with {subject}?
                3. How do students perceive {subject}?
                4. What ethical issues should be considered?
                5. How can institutions use {subject} responsibly?

                Possible thesis statements:
                1. This paper argues that {subject} can support students when it is used with clear academic guidelines.
                2. Although {subject} offers benefits, its effectiveness depends on access, training, and ethical use.
                3. {subject} should be evaluated through both learning outcomes and student responsibility.

                Recommended direction:
                - Start with a narrow question about one student group, one learning context, or one academic task.
                """
        };
    }

    private static string GenerateVietnamese(string mode, string topic, string essayType, string thesisStatement, string userInput)
    {
        string subject = FirstNonEmpty(topic, thesisStatement, userInput, "chủ đề của bạn");

        return mode switch
        {
            "Outline" => $"""
                Dàn ý học thuật cho: {subject}

                Mở bài:
                - Giới thiệu bối cảnh và lý do chủ đề quan trọng.
                - Thu hẹp phạm vi thành một vấn đề học thuật cụ thể.
                - Luận điểm dự kiến: {FirstNonEmpty(thesisStatement, "Cần xây dựng lập trường rõ ràng về " + subject)}.

                Thân bài 1:
                - Ý chính: giải thích khái niệm hoặc bối cảnh.
                - Bằng chứng cần có: nguồn học thuật, số liệu hoặc ví dụ.
                - Phân tích: liên hệ bằng chứng với luận điểm.

                Thân bài 2:
                - Ý chính: phân tích lợi ích, vấn đề hoặc nguyên nhân quan trọng.
                - Bằng chứng cần có: kết quả nghiên cứu hoặc ví dụ thực tế.

                Thân bài 3:
                - Ý chính: thảo luận tác động, ứng dụng hoặc giới hạn.

                Phản biện:
                - Trình bày một quan điểm đối lập hợp lý.

                Bác bỏ phản biện:
                - Giải thích vì sao luận điểm chính vẫn thuyết phục hơn hoặc cân bằng hơn.

                Kết luận:
                - Nhắc lại luận điểm, tóm tắt ý chính và gợi ý hướng nghiên cứu tiếp theo.
                """,
            "CounterArgument" => $"""
                Lập luận phản biện cho: {subject}

                1. Vấn đề chi phí hoặc tính khả thi:
                - Quan điểm phản đối: ý tưởng có thể khó triển khai trong bối cảnh thực tế.
                - Cách bác bỏ: thu hẹp phạm vi và nêu điều kiện để luận điểm vẫn phù hợp.

                2. Vấn đề đạo đức:
                - Quan điểm phản đối: chủ đề có thể tạo ra vấn đề công bằng, quyền riêng tư hoặc tiếp cận.
                - Cách bác bỏ: thừa nhận rủi ro và đề xuất giới hạn hoặc nguyên tắc sử dụng.

                3. Giới hạn về hiệu quả:
                - Quan điểm phản đối: giải pháp có thể không hiệu quả cho mọi nhóm hoặc mọi bối cảnh.
                - Cách bác bỏ: chỉ ra bối cảnh phù hợp và thừa nhận giới hạn còn lại.

                Câu chuyển ý gợi ý:
                - Mặc dù quan ngại này có cơ sở, các bằng chứng cho thấy rằng...
                """,
            "ThesisImprover" => $"""
                Góp ý thesis statement

                Thesis hiện tại:
                {FirstNonEmpty(thesisStatement, subject)}

                Gợi ý cải thiện:
                - Làm thesis cụ thể hơn.
                - Thêm lập trường rõ ràng, không chỉ nêu chủ đề.
                - Thêm phạm vi hoặc bối cảnh.
                - Dùng cách diễn đạt học thuật, tránh khẳng định quá rộng.

                Phiên bản cải thiện:
                1. Cơ bản: Bài viết này lập luận rằng {subject} có ý nghĩa học thuật quan trọng và cần được xem xét kỹ hơn.
                2. Học thuật hơn: Bài viết này lập luận rằng {subject} nên được phân tích qua tác động đến kết quả học tập, khả năng tiếp cận và trách nhiệm học thuật.
                3. Hướng nghiên cứu: Nghiên cứu này xem xét cách {subject} ảnh hưởng đến việc học của sinh viên và xác định điều kiện để nó phát huy hiệu quả.
                """,
            _ => $"""
                Gợi ý ý tưởng cho: {subject}

                Các góc tiếp cận:
                1. Tác động giáo dục
                2. Trải nghiệm của sinh viên
                3. Vấn đề đạo đức
                4. Công nghệ và khả năng tiếp cận
                5. Kết quả học thuật dài hạn

                Câu hỏi nghiên cứu:
                1. {subject} ảnh hưởng đến việc học của sinh viên như thế nào?
                2. Lợi ích và hạn chế của {subject} là gì?
                3. Sinh viên nhìn nhận {subject} như thế nào?
                4. Cần xem xét vấn đề đạo đức nào?
                5. Nhà trường có thể sử dụng {subject} một cách có trách nhiệm ra sao?

                Thesis statement gợi ý:
                1. Bài viết này lập luận rằng {subject} có thể hỗ trợ sinh viên nếu được sử dụng với hướng dẫn học thuật rõ ràng.
                2. Mặc dù {subject} mang lại lợi ích, hiệu quả của nó phụ thuộc vào khả năng tiếp cận, đào tạo và sử dụng có đạo đức.
                3. {subject} nên được đánh giá dựa trên cả kết quả học tập và trách nhiệm của sinh viên.

                Hướng nên chọn:
                - Hãy bắt đầu bằng một câu hỏi hẹp về một nhóm sinh viên, một bối cảnh học tập hoặc một nhiệm vụ học thuật cụ thể.
                """
        };
    }

    private static string FirstNonEmpty(params string[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
    }
}
