# Pergerakan Aset — Requirement Traceability

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12 |
| Blueprint / contract | `AST-BP-001` rev 1 · `AST-MOV-1.0.0` `approved` 2026-11-12 (`AST-DEC-051`) |
| Backend / Frontend SHA | `1f54a175` / `e49e8a81` |

Bukti backend = `V-BE`; tidak adanya automated test bukan gap (`rules/backend/TEST_POLICY.md`).

| Requirement | Decision / aturan | Desain | Task BE | Task FE | Verifikasi | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `CAP-05.1` pengajuan pemindahan | `AST-DEC-011`, `048`; `AST-RULE-012` | state §1 | `BE-AST-016` ✅, `017` ✅ | `FE-AST-012` ✅ | `AT-MOV-02`, `20..23` | `BE-AST-016`: 5 tabel, check constraint, `RowVersion`, migration diterapkan — [`BE-AST-016.md`](../task/report/backend/BE-AST-016.md). `BE-AST-017`: `AT-MOV-02` (Kepala Unit asal → `DisetujuiUnitAsal` otomatis), `20`, `21` (lewat kunci pemanfaatan), `22`, `23` lulus — [`BE-AST-017.md`](../task/report/backend/BE-AST-017.md). `FE-AST-012`: formulir + pemilih aset unit asal, aset Rusak Berat terblokir, ajukan lewat layar — [`FE-AST-012.md`](../task/report/frontend/FE-AST-012.md) | 🟡 2026-11-19 — backend dan layar ✅ (uji layar 25/25, build Rizki 12:37:20); `AT-MOV-24` menyusul `BE-AST-021` |
| `CAP-05.2` persetujuan mutasi | `AST-DEC-011`, `048`; `AST-ASM-003` | state §1 | `BE-AST-017` ✅ | `FE-AST-012` ✅ | `AT-MOV-25`, `26`, `31`; `UAT-MOV-02` | `BE-AST-017`: `AT-MOV-25` (403 `010`), `26` (403 `011`), `31` (409 `016` versi basi) lulus; persetujuan dua tahap oleh Siti (Kepala Unit Rawat Inap) dan Bagian Aset. `FE-AST-012`: Persetujuan Mutasi tab Tahap Unit Asal/Bagian Aset, tombol dari `availableActions` | ✅ 2026-11-19 — backend dan layar terbukti oleh pengembang; `UAT-MOV-02` milik tim UAT |
| `CAP-05.3` serah terima antar unit | `AST-RULE-021`; `AST-DES-023`, `024` | `INT-MOV-02` | `BE-AST-017` ✅ | `FE-AST-012` ✅ | `AT-MOV-01`, `27..29`; `UAT-MOV-01`, `03` | `BE-AST-017`: `AT-MOV-01` (penempatan pindah saat diterima, linimasa `Dimutasi`), `27`, `28`, `29` lulus; kondisi berbeda → laporan `AST-KND-000012`; BA PDF. `FE-AST-012`: modal Terima wajib ruangan + penanggung jawab, cetak BA | ✅ 2026-11-19 — backend dan layar terbukti oleh pengembang |
| Koreksi mutasi | `AST-RULE-060`; `AST-DES-025` | state §1 | `BE-AST-017` ✅ | `FE-AST-012` ✅ | `AT-MOV-03`, `30` | `BE-AST-017`: `AT-MOV-03` (rujukan mutasi asal tersimpan; baris linimasa kedua tidak teruji karena mutasi koreksi dibatalkan), `30` (batal sesudah diterima 409) lulus | ✅ 2026-11-19 — backend dan layar terbukti oleh pengembang |
| `CAP-05.4` peminjaman | `AST-DEC-012`; `AST-DES-021`, `022` | state §2–3 | `BE-AST-018` ✅ | `FE-AST-013` | `AT-MOV-04`, `05`, `32`, `34`; `UAT-MOV-04` | `BE-AST-018`: `AT-MOV-04` (`isOverdue=true`, 1 hari, pinjaman 18–18 Nov diambil 19 Nov), `05` (perpanjangan disetujui → kembali 22 Nov, Terlambat hilang), `32` (400 `020`), `34` (409 `023`) lulus; `AST-DES-021` pengajuan Kepala Unit pemilik langsung `Disetujui`; runtime 73/73 | 🟡 2026-11-19 — backend terbukti; layar `FE-AST-013` belum |
| `CAP-05.5` pengembalian | `AST-RCG-G07` | `INT-MOV-03` | `BE-AST-018` ✅ | `FE-AST-013` | `AT-MOV-06`, `33`; `UAT-MOV-05`, `06` | `BE-AST-018`: `AT-MOV-06` (Rusak Ringan → `AST-KND-000013` sumber Pengembalian Pinjaman, aset Aktif tanpa laporan), `33` (batal saat `Dipinjam` → 409 `015`) lulus; kondisi per aset wajib (400 `022`); kunci lepas, aset tetap milik pemilik, linimasa `Dipinjam`/`Dikembalikan`; BA PDF | 🟡 2026-11-19 — backend terbukti; layar `FE-AST-013` belum |
| `CAP-01.4` mutasi berjalan | — | API 2.3 | `BE-AST-019` | `FE-AST-014` | `AT-MOV-04` (kartu) | — | belum |
| `CAP-11.2` laporan mutasi | — | API 2.4 | `BE-AST-019` | `FE-AST-014` | `AT-MOV-07` | — | belum |
| Penahanan saat opname | `AST-RULE-023`; `AST-DES-026` | `INT-MOV-06` | `BE-AST-021` [INV] | — | `AT-MOV-24` | — | belum (dikerjakan `inventarisasi`) |
| Kepala Unit dari HR | `AST-UNK-004` | `INT-MOV-04` | `BE-AST-005` [REG] | — | `AT-MOV-35` | `BE-AST-017`: Kepala Unit dikenali dari penugasan manajerial aktif HR (Siti = Instalasi Rawat Inap, Dimas = IGD, dibuat lewat API HR 2026-11-19); sebelumnya DB dev tidak punya satu pun penugasan manajerial. `BE-AST-018`: `AT-MOV-35` lulus (SuperAdmin tanpa penugasan Kepala Unit pemilik → 403 `010` pada persetujuan pinjaman) | belum; data HR menahan go-live |

**Gap requirement → bukti:** tidak ada.
