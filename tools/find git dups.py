from collections import Counter, defaultdict
from dataclasses import dataclass, field
from git import Git
from re import findall, MULTILINE, sub
from typing import Callable

# https://github.com/git/git/blob/7b2bccb0d58d4f24705bf985de1f4612e4cf06e5/t/t3070-wildmatch.sh#L223

# determine duplicate patterns
@dataclass(frozen=True)
class Source:
    filename: str
    __extract_critical_token: Callable[[str], str] = field(repr=False, compare=False)
    def parse_line(self, line: str):
        token = self.__extract_critical_token(line)
        if len(token) > 0 and token[0] != '#':
            return token
        else:
            return None
        
def extract_gitattrib_pattern(line: str):
    tokens = line.split()
    if len(tokens) > 0:
        return tokens[0]
    else:
        return ''
sources = [
    Source('.gitignore', lambda line: line.strip()),
    Source('.gitattributes', extract_gitattrib_pattern)
]

def rank_char(char: str):
    priority = None
    match char:
        case '*':  priority = 0
        case '\\': priority = 1
        case '/':  priority = 2
        case '{':  priority = 3
        case '[':  priority = 4
        case '.':  priority = 5
        case _:
            if not char.isalnum():
                priority = 99
            else:
                priority = 199
    return priority, char

def rank_pattern(pattern: str, source: Source) -> tuple[bool, tuple[int, str]]:
    assert(sources[0].filename == '.gitignore')
    is_negative = source == sources[0] and pattern[0] == '!'
    if pattern == '!/TAGS/':
        print(source, pattern[0])

    pattern = sub(r'\\(.)', r'\1', pattern)
    return is_negative, tuple(map(rank_char, pattern.casefold() ))

logs: defaultdict[str, Counter[Source]] = defaultdict(Counter)
for source in sources:
    filename = source.filename
    print(f'reading {filename}')
    with open(filename, encoding='utf-8') as file:
        last_pattern = ('', tuple())
        for line in file:
            pattern = source.parse_line(line)
            if pattern != None:
                logs[pattern][source.filename] += 1
                
                rank = rank_pattern(pattern, source)
                if rank < last_pattern[1]:
                    print(f'\tout of order: {pattern} < {last_pattern[0]}')
                last_pattern = (pattern, rank)
            else:
                last_pattern = ('', tuple())
dupes = [(pattern, log) for (pattern, log) in logs.items() if log.total() > 1]

print(f'{len(dupes)} patterns have duplicate entries')
for (pattern, log) in dupes:
    print(f'\t{pattern}\t{log}')

# determine uncovered files
g = Git('./')
g.init()

attrib_table: str = g.ls_files('--eol', '--exclude-standard')
auto_files: list[str] = findall(
    r' attr/text=auto eol=lf\s+?(.*?)$',
    attrib_table,
    flags=MULTILINE
)

print(f'{ len(auto_files) } files not covered\n{ '\n'.join(auto_files) }')
