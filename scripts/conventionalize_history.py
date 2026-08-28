#!/usr/bin/env python3
"""Convert legacy commit subjects to Conventional Commit subjects.

The rewrite keeps commit bodies, authors, timestamps, trees, and parent
topology unchanged. Run the preview against a complete clone before using
the filter-repo callback.
"""

from __future__ import annotations

import re
import sys


CONVENTIONAL = re.compile(
    r"^(?:feat|fix|perf|refactor|test|docs|chore|build|ci|style|revert)"
    r"(?:\([^)]+\))?!?: .+"
)


def _subject(message: bytes) -> tuple[str, str]:
    text = message.decode("utf-8", errors="replace")
    first, separator, rest = text.partition("\n")
    return first.strip(), (separator + rest if separator else "")


def canonical_subject(subject: str) -> str:
    if CONVENTIONAL.match(subject):
        return subject

    lowered = subject.lower()
    if lowered.startswith("merge pull request"):
        return f"chore(merge): {subject.lower()}"
    if "readme" in lowered or lowered.startswith(("update license", "update docs")):
        return f"docs: {subject}"
    if lowered.startswith(("test", "add test", "improve test")) or "test coverage" in lowered:
        return f"test: {subject}"
    if any(word in lowered for word in ("perf", "optim", "latency", "throughput")):
        return f"perf: {subject}"
    if lowered.startswith(("refactor", "rename", "rework")):
        return f"refactor: {subject}"
    if lowered.startswith(("add ", "implement ", "introduce ", "extend ", "improve ")):
        return f"feat: {subject}"
    if lowered.startswith(("remove ", "clean ", "cleanup ", "update gitignore")):
        return f"chore: {subject}"
    return f"chore: {subject}"


def rewrite(message: bytes) -> bytes:
    subject, suffix = _subject(message)
    return (canonical_subject(subject) + suffix).encode("utf-8")


if __name__ == "__main__":
    for line in sys.stdin:
        line = line.rstrip("\n")
        if "\t" in line:
            sha, subject = line.split("\t", 1)
            print(f"{sha}\t{canonical_subject(subject)}")
        else:
            print(canonical_subject(line))
