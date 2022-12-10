#!/usr/bin/env node

// This script is used to verify the SMTP server by connecting to it and
// sending a simple handshake message. It is used to verify the SMTP server
// configuration in email_platforms.json. Depending on the SMTP server
// it will connect directly (STARTTLS) or via TLS.

import net from "net";
import tls from "tls";
import { readFileSync } from "fs";

const args = process.argv.slice(2);
const src = args.shift();

if (!src) {
  console.log("Usage: verify_smtp_server.mjs <email_platforms.json>");
  process.exit(1);
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
const input = JSON.parse(readFileSync(src, "utf8"));

/**
 * super simple and naive SMTP handshake probe
 * @param {string} host 
 * @param {string} port 
 * @param {boolean} useSsl 
 * @returns {Promise<{send: boolean, msg: string}[]>}
 */
function probe(host, port, useSsl, domain) {
  return new Promise((resolve, reject) => {
    const socket = useSsl
      ? tls.connect({ host, port })
      : net.connect({ host, port });

    setTimeout(() => {
      // close the socket after 15 seconds as a timeout
      socket.destroy();
    }, 1000 * 15);

    // wait for the server to send the greeting
    // send EHLO domain
    // wait for response, expect 250.
    // send QUIT
    // wait for response, expect 221.
    // close the socket

    // if useSsl is false, also check if the server supports STARTTLS, otherwise reject the promise
    // if any step got an unexpected response, close the socket and reject the promise

    /** state machine */
    let state = 0;
    /**
     * message logs, returned via Promise in the end
     * @type {{send: boolean, msg: string}[]}
     */
    const msg = [];

    // buffer for the response
    let buf = "";

    function send(data) {
      msg.push({ send: true, msg: data });
      socket.write(data + "\r\n");
    }

    function machine(data) {
      buf += data;
      const lines = buf.split("\n");
      buf = lines.pop();
      for (const line of lines) {
        msg.push({ send: false, msg: line });

        // ignore lines with a dash at the fourth position
        if (line[3] === "-") {
          continue;
        }
        const code = line.substring(0, 3);
        switch (state) {
          case 0:
            // wait for the server to send the greeting
            if (code === "220") {
              // use first domain as the EHLO domain
              send("EHLO " + domain);
              state = 1;
            } else {
              socket.end();
              reject(msg);
            }
            break;
          case 1:
            // send EHLO mail.example.com
            // wait for response, expect 250.
            if (code === "250") {
              send("QUIT");
              state = 2;
            } else {
              socket.end();
              reject(msg);
            }
            break;
          case 2:
            // send QUIT
            // wait for response, expect 221.
            if (code === "221") {
              socket.end();
              resolve(msg);
            } else {
              socket.end();
              reject(msg);
            }
            break;
          default:
            socket.end();
            reject("Unexpected state");
        }
      }
    }

    socket.on("connect", () => {
      console.log(`Connected to ${host}:${port} using ${useSsl ? "TLS" : "TCP"}`);
      socket.on("data", machine);
    });

    socket.on("error", (e) => {
      reject(e);
    });

    socket.on("close", () => {
      console.log(`Connection to ${host}:${port} closed`);
      reject(msg);
    });
  });
}

(async () => {
  // record host, port and if it was successful
  let summary = [];

  for (const entry of input) {
    const { host, port, useSsl, domains } = entry;
    console.log("=========================================");
    console.log(`Probing ${host}:${port} using ${useSsl ? "TLS" : "TCP"}`);
    try {
      const domain = domains[0];
      const result = await probe(host, port, useSsl, domain);
      console.log(result.map((e) => (e.send ? "S: " : "R: ") + e.msg).join("\n"));

      // if useSsl is false, check for STARTTLS
      if (!useSsl) {
        const starttls = result.find((e) => e.msg.includes("250-STARTTLS") || e.msg.includes("250 STARTTLS"));
        if (!starttls) {
          console.error(`SMTP server ${host}:${port} does not support STARTTLS`);
          summary.push({ host, port, useSsl, success: false });
          continue;
        }
      }

      console.log(`SMTP server ${host}:${port} is reachable`);
      summary.push({ host, port, useSsl, success: true });

    } catch (e) {
      // if the error is the message log, pretty print it
      if (Array.isArray(e) && e.length > 0 && e[0].hasOwnProperty("send")) {
        console.error(e.map((e) => (e.send ? "S: " : "R: ") + e.msg).join("\n"));
      } else {
        console.error(e);
      }
      summary.push({ host, port, useSsl, success: false });
      console.error(`SMTP server ${host}:${port} is not reachable`);
    }
  }

  console.log("=========================================");
  console.log("Summary:");
  console.log(summary.map((e) => `${e.success ? "[ OK ]" : "[FAIL]"} ${e.useSsl ? "TLS" : "TCP"} ${e.host}:${e.port}`).join("\n"));
})();
