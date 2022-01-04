import wikiLinkParser from "./wikiLinkParser";
import { it, expect } from "@jest/globals";

it("parses lorem ipsum with https", () => {
  const result = wikiLinkParser("https://en.wikipedia.org/wiki/Event_storming");

  expect(result).not.toBeNull();
  expect(result.valid).toBeTruthy();
  expect(result.id).toEqual("Event_storming");
});

it("parses lorem ipsum without https", () => {
  const result = wikiLinkParser("en.wikipedia.org/wiki/Event_storming");

  expect(result).not.toBeNull();
  expect(result.valid).toBeTruthy();
  expect(result.id).toEqual("Event_storming");
});
