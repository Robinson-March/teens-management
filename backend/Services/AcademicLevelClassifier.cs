namespace TeensChurch.API.Services;

public static class AcademicLevelClassifier
{
    // Standard Canonical Dropdown Option Values
    public const string Jss1 = "Junior Secondary (JSS1)";
    public const string Jss2 = "Junior Secondary (JSS2)";
    public const string Jss3 = "Graduating JSS (JSS3)";
    public const string Ss1 = "Senior Secondary (SS1)";
    public const string Ss2 = "Senior Secondary (SS2)";
    public const string Ss3 = "Senior Secondary (SS3)";
    public const string AdmissionSeekerLevel = "Admission seeker";
    public const string PreVarsityLevel = "Pre-Varsity (A-Level)";
    public const string TertiaryLevel = "Undergraduate (100L-400L)";
    public const string WorkingVocationalLevel = "Working / Vocational";

    // Dashboard Analytics Cohort Distribution Categories
    public const string JuniorTeens = "Junior Teens (JSS1-2)";
    public const string GraduatingJss = "Graduating JSS (JSS3)";
    public const string SeniorTeens = "Senior Teens (SS1)";
    public const string ExamClass = "Exam Class (SS2/SS3)";
    public const string AdmissionSeeker = "Admission / Pre-Varsity";
    public const string Tertiary = "Tertiary (Undergraduate)";
    public const string WorkingVocational = "Working / Vocational";
    public const string Unspecified = "Unspecified / Other";

    /// <summary>
    /// Normalizes raw CSV or user-submitted academic level variations into clean, standard canonical representations.
    /// Handles all formats from the CSV: SS1, SS.1, S.S.1, SS 1, Science, SS2, S.S.2, S.S. II, SS3, Schooling SS3,
    /// JSS2, JSS3, JSS 3, Admission seeker, Admission, Admission Seaker, Addmission Seeker, 100LV, 400L,
    /// working, worker, A WORKER, teaching / working, teaching/working, work, etc.
    /// </summary>
    public static string? NormalizeLevel(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var s = raw.Trim().ToLowerInvariant();

        if (s == "not provided" || s == "n/a" || s == "none" || s == "-" || s == "null" || s == "nil")
            return null;

        // 1. JSS 1
        if (s == "jss1" || s == "jss 1" || s == "jss.1" || s == "j.s.s.1" || s.Contains("jss1") || s.Contains("jss 1"))
        {
            if (!s.Contains("jss2") && !s.Contains("jss 2") && !s.Contains("jss3") && !s.Contains("jss 3"))
                return Jss1;
        }

        // 2. JSS 2
        if (s == "jss2" || s == "jss 2" || s == "jss.2" || s == "j.s.s.2" || s.Contains("jss2") || s.Contains("jss 2"))
            return Jss2;

        // 3. JSS 3 (Graduating JSS)
        if (s == "jss3" || s == "jss 3" || s == "jss.3" || s == "j.s.s.3" || s.Contains("jss3") || s.Contains("jss 3") || s.Contains("bece") || s.Contains("graduating jss"))
            return Jss3;

        // 4. SS 3 / Final Year High School (WAEC / NECO candidates)
        if (s == "ss3" || s == "ss 3" || s == "s.s.3" || s == "ss.3" || s == "s.s 3" || s == "sss 3" || s == "sss3" ||
            s.Contains("ss3") || s.Contains("ss 3") || s.Contains("schooling ss3") || s.Contains("waec") || s.Contains("neco"))
            return Ss3;

        // 5. SS 2 / S.S. II / Pre-Exam Candidates
        if (s == "ss2" || s == "ss 2" || s == "s.s.2" || s == "ss.2" || s == "s.s. ii" || s == "s.s ii" || s == "ss ii" || s == "sss 2" || s == "sss2" ||
            s.Contains("ss2") || s.Contains("ss 2") || s.Contains("s.s. ii") || s.Contains("s.s ii") || s.Contains("ss ii"))
            return Ss2;

        // 6. SS 1 / Science / Arts / Commercial Stream
        if (s == "ss1" || s == "ss 1" || s == "s.s.1" || s == "ss.1" || s == "s.s 1" || s == "sss 1" || s == "sss1" || s == "science" ||
            s.Contains("ss1") || s.Contains("ss 1") || s.Contains("s.s.1") || s.Contains("ss.1"))
            return Ss1;

        // 7. Admission Seekers / JAMB candidates
        if (s.Contains("admission") || s.Contains("addmission") || s.Contains("jamb") || s.Contains("utme") || s.Contains("jambite"))
            return AdmissionSeekerLevel;

        // 8. Pre-Varsity / A-Level / Gap Year
        if (s.Contains("pre-varsity") || s.Contains("a-level") || s.Contains("alevel") || s.Contains("gap year") || s.Contains("ijmb") || s.Contains("jupeb"))
            return PreVarsityLevel;

        // 9. Tertiary / Undergraduate / 100LV - 400L
        if (s.Contains("100l") || s.Contains("200l") || s.Contains("300l") || s.Contains("400l") || s.Contains("500l") ||
            s.Contains("tertiary") || s.Contains("undergraduate") || s.Contains("varsity") || s.Contains("poly") || s.Contains("university"))
            return TertiaryLevel;

        // 10. Working / Vocational / Apprentice / Teacher
        if (s.Contains("work") || s.Contains("worker") || s.Contains("teach") || s.Contains("trade") || s.Contains("apprentice") || s.Contains("vocation"))
            return WorkingVocationalLevel;

        return raw.Trim();
    }

    /// <summary>
    /// Categorizes an academic level (normalized or raw) into one of 7 standardized cohort segments for analytics.
    /// </summary>
    public static string Categorize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Unspecified;

        var s = raw.Trim().ToLowerInvariant();

        if (s == "not provided" || s == "n/a" || s == "none" || s == "-" || s == "null" || s == "nil")
            return Unspecified;

        // 1. Junior Teens (JSS 1 & JSS 2)
        if (s.Contains("jss 1") || s.Contains("jss1") || s.Contains("jss 2") || s.Contains("jss2"))
            return JuniorTeens;

        // 2. Graduating JSS (JSS 3)
        if (s.Contains("jss 3") || s.Contains("jss3") || s.Contains("bece"))
            return GraduatingJss;

        // 3. Senior Exam Class (SS 2 & SS 3) - WAEC, NECO & JAMB candidates
        if (s.Contains("ss 3") || s.Contains("ss3") || s.Contains("s.s.3") || s.Contains("ss.3") || s.Contains("sss 3") || s.Contains("sss3") || s.Contains("schooling ss3") ||
            s.Contains("ss 2") || s.Contains("ss2") || s.Contains("s.s.2") || s.Contains("s.s. ii") || s.Contains("s.s ii") || s.Contains("sss 2") || s.Contains("sss2"))
            return ExamClass;

        // 4. Senior Teens (SS 1)
        if (s.Contains("ss 1") || s.Contains("ss1") || s.Contains("s.s.1") || s.Contains("ss.1") || s.Contains("sss 1") || s.Contains("sss1") || s == "science")
            return SeniorTeens;

        // 5. Admission Seekers & Pre-Varsity Candidates
        if (s.Contains("admission") || s.Contains("addmission") || s.Contains("jamb") || s.Contains("pre-varsity") || s.Contains("a-level") || s.Contains("jambite") || s.Contains("gap year"))
            return AdmissionSeeker;

        // 6. Tertiary / University / Polytechnic
        if (s.Contains("100l") || s.Contains("200l") || s.Contains("300l") || s.Contains("400l") || s.Contains("500l") ||
            s.Contains("tertiary") || s.Contains("varsity") || s.Contains("poly") || s.Contains("undergraduate") || s.Contains("university"))
            return Tertiary;

        // 7. Working / Apprenticeship / Young Workers
        if (s.Contains("work") || s.Contains("teach") || s.Contains("worker") || s.Contains("apprentice") || s.Contains("trade"))
            return WorkingVocational;

        return Unspecified;
    }

    /// <summary>
    /// Checks whether an academic level qualifies as Senior High or Candidates (SS1, SS2, SS3, or Admission Seeker).
    /// </summary>
    public static bool IsSeniorOrCandidate(string? raw)
    {
        var category = Categorize(raw);
        return category is SeniorTeens or ExamClass or AdmissionSeeker;
    }
}
