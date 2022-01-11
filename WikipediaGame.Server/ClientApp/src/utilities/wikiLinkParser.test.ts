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

it("parses lorem ipsum with https in mobile", () => {
  const result = wikiLinkParser("https://en.m.wikipedia.org/wiki/Event_storming");

  expect(result).not.toBeNull();
  expect(result.valid).toBeTruthy();
  expect(result.id).toEqual("Event_storming");
});
it("parses lorem ipsum without https in mobile", () => {
  const result = wikiLinkParser("en.m.wikipedia.org/wiki/Event_storming");

  expect(result).not.toBeNull();
  expect(result.valid).toBeTruthy();
  expect(result.id).toEqual("Event_storming");
});

it("ignores # after the url segment ends", () => {
  const result = wikiLinkParser("en.m.wikipedia.org/wiki/Event_storming#hello");

  expect(result).not.toBeNull();
  expect(result.valid).toBeTruthy();
  expect(result.id).toEqual("Event_storming");
});
