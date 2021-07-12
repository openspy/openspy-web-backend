public static class IRCMatch {
    private static string Increment(string s, int inc) {
        if(s == null) return null;
        try {
            return s.Substring(inc);
        } catch (System.ArgumentOutOfRangeException) {
            return null;
        }
    }
    private static int match2(string mask, string name) {
        string m = mask;
        string n = name;
        string ma = null;
        string na = name;
        while(true) {
            if(!string.IsNullOrWhiteSpace(m) && m[0] == '*') {
                while(!string.IsNullOrWhiteSpace(m) && m[0] == '*') /* collapse.. */
                    m = Increment(m, 1);
                ma = m;
                na = n;
            }
            if(string.IsNullOrWhiteSpace(m)) {
                if(string.IsNullOrWhiteSpace(n))   
                    return 0;
                if(ma == null)
                    return 1;
                //for (m--; (m > (const unsigned char *)mask) && (*m == '?'); m--);
                if(!string.IsNullOrWhiteSpace(m) && m[0] == '*')
                    return 0;
                m = ma;
                n = Increment(na, 1); na = n;
            } else if(n == null) {
                while(string.IsNullOrWhiteSpace(m) && m[0] == '*') /* collapse.. */
                    m = Increment(m, 1);
                return m != null ? 1 : 0;
            }
            if((string.IsNullOrWhiteSpace(m) || string.IsNullOrWhiteSpace(n)) || char.ToLower(m[0]) != char.ToLower(n[0]) && !((m[0] == '_') && (n[0] == ' ')) && (m[0] != '?')) {
                if(ma == null)
                    return 1;
                m = ma;
                n = Increment(na, 1);  na = n;
            } else {
                m = Increment(m, 1);
                n = Increment(n, 1);
            }
        }
    }
    public static int match(string mask, string name) {
        if(mask.Length < 2) return match2(mask, name);
        if(mask[0] == '*' && mask[1] == '!') {
            mask = Increment(mask, 2);
            while(name[0] != '!' && name.Length > 1) {
                name = Increment(name, 1);
                if(string.IsNullOrWhiteSpace(name))
                    return 1;
            }
        }

        if(mask.Length < 2) return match2(mask, name);

        if(mask[0] == '*' && mask[1] == '@') {
            mask = Increment(mask, 2);
            while(!string.IsNullOrWhiteSpace(name) && name[0] != '!') {
                name = Increment(name, 1);
                if(string.IsNullOrWhiteSpace(name))
                    return 1;
                name = Increment(name, 1);
            }
        }
        return match2(mask, name);
    }
}