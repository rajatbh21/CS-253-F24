#!/bin/bash
stop_words=$(tr ',' '\n' < ../stop_words.txt; echo {a..z})
grep -oE '\w+' "$1" | tr '[:upper:]' '[:lower:]' | grep -vwF "$stop_words" | sort | uniq -c | sort -nr | head -25 | awk '{print $2, "-", $1}'
