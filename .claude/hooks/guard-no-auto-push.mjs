#!/usr/bin/env node
// PreToolUse hook — penegak `.claude/rules/no-auto-push.md`.
//
// Aturannya satu kalimat: Claude TIDAK PERNAH menjalankan `git push` di repo
// backend. Titik publikasi ke remote dipegang user sendiri.
//
// Hook ini sengaja lebih keras daripada `guard-backend-push.mjs` milik repo
// frontend. Hook itu masih mengizinkan `push origin MHamzah`; hook ini menolak
// SEMUA push ke backend, termasuk yang tujuannya benar. Keduanya boleh aktif
// berdampingan — yang menolak lebih dulu yang menang.
//
// Yang TIDAK diperiksa (semuanya lokal dan tetap bebas dipakai):
// add, commit, status, log, diff, pull, fetch, rebase, checkout, reset,
// stash, dotnet build/run/ef, dan operasi tulis file.

import { stdin } from "node:process";

const BACKEND = /quilvian_?backend|quilviansystembackend/i;
const FRONTEND = /quilvian_?frontend|quilviansystemfrontend/i;
const RULES = "QuilvianBackend/.claude/rules/no-auto-push.md";

// Operasi `gh` yang efeknya setara mem-publish ke remote tanpa lewat `git push`.
const GH_RISKY = [
  /(?:^|[\s;&|(])gh\s+pr\s+(?:create|merge|close)\b/i,
  /(?:^|[\s;&|(])gh\s+release\b/i,
  /(?:^|[\s;&|(])gh\s+repo\s+(?:delete|rename|archive)\b/i,
  /(?:^|[\s;&|(])gh\s+api\b[^\n]*(?:-X|--method)\s*(?:POST|PUT|PATCH|DELETE)\b/i,
];

function deny(reason) {
  process.stdout.write(
    JSON.stringify({
      hookSpecificOutput: {
        hookEventName: "PreToolUse",
        permissionDecision: "deny",
        permissionDecisionReason: `${reason} Lihat ${RULES}`,
      },
      systemMessage: `🔒 Push backend diblokir: ${reason}`,
    }),
  );
  process.exit(0);
}

// Pemisah antar perintah dalam satu string shell. Cukup untuk melacak `cd` yang
// mendahului `git push` — ini bukan parser shell lengkap.
const splitSegments = (command) => command.split(/\|\||&&|[;\n\r|&]/);

const tokenize = (segment) =>
  (segment.match(/"[^"]*"|'[^']*'|\S+/g) ?? []).map((token) =>
    token.replace(/^["']|["']$/g, ""),
  );

const dirKind = (path) => {
  if (!path) return null;
  if (BACKEND.test(path)) return "backend";
  if (FRONTEND.test(path)) return "frontend";
  return null;
};

let raw = "";
for await (const chunk of stdin) raw += chunk;

let payload;
try {
  payload = JSON.parse(raw);
} catch {
  process.exit(0); // tidak ada yang bisa diperiksa
}

const command = payload?.tool_input?.command;
if (typeof command !== "string" || !command) process.exit(0);

// Konteks direktori awal: cwd sesi, lalu diperbarui setiap kali ada `cd`.
let context = dirKind(payload?.cwd ?? "");

for (const segment of splitSegments(command)) {
  const tokens = tokenize(segment);
  if (tokens.length === 0) continue;

  const cdIndex = tokens.findIndex((t) => /^(?:cd|pushd|Set-Location|sl)$/i.test(t));
  if (cdIndex !== -1) {
    const target = dirKind(tokens[cdIndex + 1]);
    if (target) context = target; // `cd ..` / path lain: konteks dibiarkan
  }

  const gitIndex = tokens.findIndex((t) => /(?:^|[\\/])git(?:\.exe)?$/i.test(t));
  if (gitIndex === -1) continue;

  // Opsi global git sebelum verb: -C <path>, -c <cfg>, --git-dir=..., dst.
  let cursor = gitIndex + 1;
  let repoDir = null;
  while (cursor < tokens.length) {
    const token = tokens[cursor];
    if (/^(?:-C|-c|--git-dir|--work-tree|--namespace)$/.test(token)) {
      if (token === "-C") repoDir ??= tokens[cursor + 1] ?? null;
      cursor += 2;
      continue;
    }
    if (/^(?:--git-dir|--work-tree)=/.test(token)) {
      repoDir ??= token.split("=")[1];
      cursor += 1;
      continue;
    }
    if (token.startsWith("-")) {
      cursor += 1;
      continue;
    }
    break;
  }

  const verb = tokens[cursor];
  if (!verb) continue;
  const args = tokens.slice(cursor + 1);

  // Repo mana yang disentuh: `-C <path>` menang; kalau tidak ada, pakai konteks
  // cwd; kalau konteks tidak diketahui, jatuh ke penyebutan nama di perintah.
  const repo = repoDir
    ? dirKind(repoDir)
    : (context ?? (BACKEND.test(command) ? "backend" : null));
  if (repo !== "backend") continue;

  if (verb === "remote" && /^(?:add|remove|rm|set-url|rename)$/i.test(args[0] ?? "")) {
    deny("Mengubah remote repo backend bukan keputusan Claude.");
  }

  if (verb === "push") {
    deny(
      "Claude tidak menjalankan `git push` di backend — user yang melakukannya sendiri. " +
        "Selesaikan sampai `git commit`, lalu laporkan sha commit dan perintah push-nya.",
    );
  }
}

// Konteks backend bisa datang dari dua arah: nama repo disebut di perintah, atau
// cwd/`cd` memang sudah berada di dalam backend. `gh pr create` polos yang
// dijalankan dari dalam folder backend tidak menyebut nama repo sama sekali.
if (BACKEND.test(command) || context === "backend") {
  const risky = GH_RISKY.find((pattern) => pattern.test(command));
  if (risky) {
    deny(
      "Operasi GitHub CLI ini mempublikasikan perubahan ke remote tanpa lewat `git push`, " +
        "sehingga ikut dilarang. Serahkan ke user.",
    );
  }
}

process.exit(0);
