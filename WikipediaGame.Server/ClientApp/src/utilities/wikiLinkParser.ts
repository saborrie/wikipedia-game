//https://en.wikipedia.org/wiki/

export interface ParseResult {
  valid: boolean;
  id?: string;
}

// this function could also be done by using `new URL(url)` and inspecting

export default function parseLink(url: string): ParseResult {
  const match = /^(?:https\:\/\/)?en(?:\.m)?\.wikipedia\.org\/wiki\/(.*?)(?:[#?].*?)?$/.exec(url);

  if (match) {
    return { valid: true, id: match[1] };
  }
  return { valid: false };
}
