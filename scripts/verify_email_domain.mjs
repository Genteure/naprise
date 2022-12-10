#!/usr/bin/env node

// This script is used to verify the email domain in email_platforms.json
// by resolving the MX records. It will output the result in JSON format
// that makes it easy to glance through the result and confirm the domains
// in a group have the same or similar MX records.

import { Resolver } from "dns/promises";
import { readFileSync } from "fs";

const r = new Resolver();
var args = process.argv.slice(2);
const src = args.shift();

if (!src) {
  console.log("Usage: verify_email_domain.mjs <email_platforms.json> [DNS server...]");
  process.exit(1);
}

if (args.length > 0) {
  console.log("Using custom DNS servers: " + args);
  r.setServers(args);
} else {
  console.log("Using default DNS servers");
}

/*
Sample data:
[
  {
    "name": "Gmail",
    "host": "smtp.gmail.com",
    "port": 587,
    "useSsl": true,
    "userNameWithDomain": true,
    "domains": ["gmail.com"]
  },
]
*/

/** @type {{name: string, host: string, port: number, useSsl: boolean, userNameWithDomain: boolean, domains: string[] }[]} */
const input = JSON.parse(readFileSync(src, 'utf8'));

// wrapper for r.resolveMx() to try again if failed
async function resolveMx(domain) {
  let f = true;
  while (true) {
    try {
      return await r.resolveMx(domain);
    }
    catch (e) {
      if (f) {
        // retry a second time
        f = false;
      } else {
        // give up
        console.log(`Error resolving MX for ${domain}: ${e}`);
        return [];
      }
    }
  }
}

/*
Target output:

{
  "Gmail": {
    "gmail.com": ["gmail-smtp-in.l.google.com", "alt1.gmail-smtp-in.l.google.com"],
    "example.com": "mx.example.com",
  }
}
*/

(async () => {
  const result = {};

  for (const { name, domains } of input) {

    console.log(`Resolving MX for ${name}...`);
    console.log(`Domain count: ${domains.length}`)

    for (const domain of domains) {
      // resolve MX records
      const records = await resolveMx(domain);
      // if there is any MX record
      if (records.length > 0) {
        // create the name group if not exists
        if (!result[name]) {
          result[name] = {};
        }
        // if there is only one MX record, use the string value
        result[name][domain] = records.length === 1 ? records[0].exchange : records.map(r => r.exchange);
      }
    }

    // sort domains by number of MX records ascending
    // string value count as 1
    result[name] = Object.fromEntries(Object.entries(result[name]).sort((a, b) => {
      const aCount = typeof a[1] === 'string' ? 1 : a[1].length;
      const bCount = typeof b[1] === 'string' ? 1 : b[1].length;
      return aCount - bCount;
    }));

  }

  console.log(JSON.stringify(result, null, 2));
})();
