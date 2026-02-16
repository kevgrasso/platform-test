from collections import Counter, defaultdict
from dataclasses import dataclass, field
from typing import Callable

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

logs: defaultdict[str, Counter[Source]] = defaultdict(Counter)
for source in sources:
    with open(source.filename, encoding="utf-8") as file:
        for line in file:
            pattern = source.parse_line(line)
            if pattern != None:
                logs[pattern][source.filename] += 1
dupes = [(pattern, log) for (pattern, log) in logs.items() if log.total() > 1]

print(f"{len(dupes)} patterns have duplicate entries:")
for (pattern, log) in dupes:
    print(f"{pattern}\t{log}")