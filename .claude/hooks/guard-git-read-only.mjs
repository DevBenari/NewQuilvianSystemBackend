#!/usr/bin/env node
// PreToolUse hook — penegak `.claude/rules/git-read-only.md`.
//
// Di repo backend, Claude hanya boleh MEMBACA state git. Seluruh perintah yang
// mengubah index, working tree, branch, atau remote — add, commit, push, pull,
// fetch, checkout, branch, reset, restore, stash, merge, rebase — dijalankan
// user sendiri.
//
// Pendekatannya ALLOWLIST, bukan denylist: hanya verb yang terbukti hanya-baca
// yang diloloskan. Verb yang belum terpikir saat hook ini ditulis ikut tertahan,
// bukan lolos diam-diam.
//
// Yang TIDAK diperiksa sama sekali: perintah non-git (dotnet build/run/ef, tulis
// file, rg, ls) dan seluruh perintah di repo frontend.

import { stdin } from "node:process";

const BACKEND = /quilvian_?backend|quilviansystembackend/i;
const FRONTEND = /quilvian_?frontend|quilviansystemfrontend/i;
const RULES = "QuilvianBackend/.claude/rules/git-read-only.md";

// Verb git yang murni menampilkan keadaan. Semua yang di luar daftar ini ditolak
// saat konteksnya repo backend.
const READ_ONLY_VERBS = new Set([
  "status",
  "log",
  "diff",
  "show",
  "blame",
  "rev-parse",
  "rev-list",
  "ls-files",
  "ls-tree",
  "ls-remote",
  "cat-file",
  "describe",
  "shortlog",
  "name-rev",
  "for-each-ref",
  "symbolic-ref",
  "check-ignore",
  "count-objects",
  "whatchanged",
  "grep",
  "version",
  "help",
]);

// Verb yang punya bentuk baca DAN bentuk tulis — hanya diloloskan kalau
// subcommand/flag-nya terbukti baca.
const CONDITIONAL_VERBS = {
  remote: (args) =>
    args.length === 0 ||
    /^(?:show|get-url|-v|--verbose)$/i.test(args[0]),
  config: (args) => args.some((a) => /^--(?:get|get-all|get-regexp|list|l)$/i.test(a)),
  reflog: (args) => args.length === 0 || /^show$/i.test(args[0]),
  notes: (args) => args.length === 0 || /^(?:list|show)$/i.test(args[0]),
};

// Operasi `gh` yang efeknya setara mem-publish ke remote tanpa lewat git.
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
      systemMessage: `🔒 Git backend hanya-baca: ${reason}`,
    }),
  );
  process.exit(0);
}

// Pemisah antar perintah dalam satu string shell. Cukup untuk melacak `cd` yang
// mendahului perintah git — ini bukan parser shell lengkap.
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

  if (READ_ONLY_VERBS.has(verb)) continue;

  const conditional = CONDITIONAL_VERBS[verb];
  if (conditional) {
    if (conditional(args)) continue;
    deny(
      `\`${`git ${verb} ${args[0] ?? ""}`.trim()}\` mengubah konfigurasi repo backend — ` +
        "Claude hanya boleh membacanya.",
    );
  }

  deny(
    `\`git ${verb}\` di backend dijalankan user sendiri, bukan Claude. ` +
      "Claude hanya boleh membaca state git (status, log, diff, show, rev-parse). " +
      "Selesaikan sampai file tertulis dan build lolos, lalu sajikan perintah git-nya.",
  );
}

// Konteks backend bisa datang dari dua arah: nama repo disebut di perintah, atau
// cwd/`cd` memang sudah berada di dalam backend. `gh pr create` polos yang
// dijalankan dari dalam folder backend tidak menyebut nama repo sama sekali.
if (BACKEND.test(command) || context === "backend") {
  const risky = GH_RISKY.find((pattern) => pattern.test(command));
  if (risky) {
    deny(
      "Operasi GitHub CLI ini mengubah state repo remote tanpa lewat git, " +
        "sehingga ikut dilarang. Serahkan ke user.",
    );
  }
}

process.exit(0);
