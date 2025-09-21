namespace Pwgen
{
    using System;
    using System.Text;

    // Статический класс с флагами для удобного использования извне
    public static class PasswordFlags
    {
        public const int Ambiguous = 1;
        public const int Uppers = 2;
        public const int Digits = 4;
        public const int Symbols = 8;
    }

    public class PhoneticPasswordGenerator
    {
        private Random random = new Random();

        // Флаги для типов элементов (остаются внутри класса)
        private const int VOWEL = 1;
        private const int CONSONANT = 2;
        private const int DIPTHONG = 4;
        private const int NOT_FIRST = 8;

        // Неоднозначные символы
        private const string pw_ambiguous = "B8G6I1l0OQDS5Z2";

        // Символы для вставки
        private const string pw_symbols = "!@#$%^&*()";

        // Элементы для генерации
        private readonly (string str, int flags)[] elements = new (string, int)[]
        {
        ( "a",  VOWEL ),
        ( "ae", VOWEL | DIPTHONG ),
        ( "ah", VOWEL | DIPTHONG ),
        ( "ai", VOWEL | DIPTHONG ),
        ( "b",  CONSONANT ),
        ( "c",  CONSONANT ),
        ( "ch", CONSONANT | DIPTHONG ),
        ( "d",  CONSONANT ),
        ( "e",  VOWEL ),
        ( "ee", VOWEL | DIPTHONG ),
        ( "ei", VOWEL | DIPTHONG ),
        ( "f",  CONSONANT ),
        ( "g",  CONSONANT ),
        ( "gh", CONSONANT | DIPTHONG | NOT_FIRST ),
        ( "h",  CONSONANT ),
        ( "i",  VOWEL ),
        ( "ie", VOWEL | DIPTHONG ),
        ( "j",  CONSONANT ),
        ( "k",  CONSONANT ),
        ( "l",  CONSONANT ),
        ( "m",  CONSONANT ),
        ( "n",  CONSONANT ),
        ( "ng", CONSONANT | DIPTHONG | NOT_FIRST ),
        ( "o",  VOWEL ),
        ( "oh", VOWEL | DIPTHONG ),
        ( "oo", VOWEL | DIPTHONG),
        ( "p",  CONSONANT ),
        ( "ph", CONSONANT | DIPTHONG ),
        ( "qu", CONSONANT | DIPTHONG),
        ( "r",  CONSONANT ),
        ( "s",  CONSONANT ),
        ( "sh", CONSONANT | DIPTHONG),
        ( "t",  CONSONANT ),
        ( "th", CONSONANT | DIPTHONG),
        ( "u",  VOWEL ),
        ( "v",  CONSONANT ),
        ( "w",  CONSONANT ),
        ( "x",  CONSONANT ),
        ( "y",  CONSONANT ),
        ( "z",  CONSONANT )
        };

        public string GeneratePhoneticPassword(int size, int pwFlags)
        {
            string result;
            int featureFlags;

            do
            {
                result = GeneratePasswordAttempt(size, pwFlags, out featureFlags);
            }
            while ((featureFlags & (PasswordFlags.Uppers | PasswordFlags.Digits | PasswordFlags.Symbols)) != 0);

            return result;
        }

        private string GeneratePasswordAttempt(int size, int pwFlags, out int featureFlags)
        {
            StringBuilder buf = new StringBuilder(size);
            featureFlags = pwFlags;
            int prev = 0;
            int shouldBe = random.Next(2) == 0 ? VOWEL : CONSONANT;
            bool first = true;

            while (buf.Length < size)
            {
                int i = random.Next(elements.Length);
                string str = elements[i].str;
                int flags = elements[i].flags;
                int len = str.Length;

                // Фильтрация по базовому типу следующего элемента
                if ((flags & shouldBe) == 0)
                    continue;

                // Обработка флага NOT_FIRST
                if (first && (flags & NOT_FIRST) != 0)
                    continue;

                // Не допускать VOWEL, за которым следует пара Vowel/Dipthong
                if ((prev & VOWEL) != 0 && (flags & VOWEL) != 0 && (flags & DIPTHONG) != 0)
                    continue;

                // Не переполнять буфер
                if (len > size - buf.Length)
                    continue;

                // Обработка флага Ambiguous
                if ((pwFlags & PasswordFlags.Ambiguous) != 0)
                {
                    bool hasAmbiguous = false;
                    foreach (char c in str)
                    {
                        if (pw_ambiguous.IndexOf(c) >= 0)
                        {
                            hasAmbiguous = true;
                            break;
                        }
                    }
                    if (hasAmbiguous)
                        continue;
                }

                // Добавление элемента в буфер
                buf.Append(str);

                // Обработка Uppers
                if ((pwFlags & PasswordFlags.Uppers) != 0)
                {
                    if ((first || (flags & CONSONANT) != 0) && random.Next(10) < 2)
                    {
                        buf[buf.Length - len] = char.ToUpper(buf[buf.Length - len]);
                        featureFlags &= ~PasswordFlags.Uppers;
                    }
                }

                // Проверка, достигнут ли размер
                if (buf.Length >= size)
                    break;

                // Обработка Digits
                if ((pwFlags & PasswordFlags.Digits) != 0)
                {
                    if (!first && random.Next(10) < 3)
                    {
                        char digit;
                        do
                        {
                            digit = (char)('0' + random.Next(10));
                        }
                        while ((pwFlags & PasswordFlags.Ambiguous) != 0 && pw_ambiguous.IndexOf(digit) >= 0);

                        buf.Append(digit);
                        featureFlags &= ~PasswordFlags.Digits;

                        first = true;
                        prev = 0;
                        shouldBe = random.Next(2) == 0 ? VOWEL : CONSONANT;
                        continue;
                    }
                }

                // Обработка Symbols
                if ((pwFlags & PasswordFlags.Symbols) != 0)
                {
                    if (!first && random.Next(10) < 2)
                    {
                        char symbol;
                        do
                        {
                            symbol = pw_symbols[random.Next(pw_symbols.Length)];
                        }
                        while ((pwFlags & PasswordFlags.Ambiguous) != 0 && pw_ambiguous.IndexOf(symbol) >= 0);

                        buf.Append(symbol);
                        featureFlags &= ~PasswordFlags.Symbols;
                    }
                }

                // Определение следующего элемента
                if (shouldBe == CONSONANT)
                {
                    shouldBe = VOWEL;
                }
                else
                {
                    if ((prev & VOWEL) != 0 || (flags & DIPTHONG) != 0 || random.Next(10) > 3)
                        shouldBe = CONSONANT;
                    else
                        shouldBe = VOWEL;
                }

                prev = flags;
                first = false;
            }

            return buf.ToString();
        }
    }
}
