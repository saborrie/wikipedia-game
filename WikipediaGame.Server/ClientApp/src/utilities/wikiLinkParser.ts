//https://en.wikipedia.org/wiki/

export interface ParseResult {
  valid: boolean;
  id?: string;
}

export default function parseLink(url: string): ParseResult {
  const match = /^(https\:\/\/)?en\.wikipedia\.org\/wiki\/(.*?)$/.exec(url);

  if (match) {
    return { valid: true, id: match[2] };
  }
  return { valid: false };
}
