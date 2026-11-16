# Laporan Perubahan Frontend — `FE-HRD-R23`

## Metadata

| Field | Nilai |
| --- | --- |
| Task / judul | `FE-HRD-R23` — Lembur bagian 1: pengajuan dan persetujuan ruangan |
| Slice / trace | `S-B3`; `FE-HRD-117`–`120` |
| Kontrak / dependency | API Contract v11 bagian 17 (`approved`); `FE-HRD-R11` ✅ |
| Commit | FE `ec3616405`; BE `7b2b15ccd` (read-only) |
| Tanggal / status | 16 November 2026 / 🟡 **SEBAGIAN** |

## Hasil

ESS lembur dan meja persetujuan ruangan memakai slice/service nyata untuk SPKL terencana, klaim cito, riwayat, detail, pembatalan, setuju, dan tolak. Kuota dihitung dari respons setting/riwayat server. Guard anti-self-approval memeriksa pengaju dan anggota SPKL di UI; server tetap menjadi otoritas akhir. Bulk approval menjalankan endpoint approve per tiket dan menampilkan hasil tiap item karena backend tidak memiliki endpoint bulk atomik.

## UI gate dan verifikasi

`UI GATE: PASS — 7 berkas target, 0 raw form/table/button, 0 Tailwind; base component dan CSS Module bertoken.`

| Pemeriksaan | Hasil |
| --- | --- |
| `npm run lint` | PASS — 0 errors; 1252 warning existing |
| `npm run build` | PASS — 814 static pages |
| SPKL/klaim cito/riwayat | API nyata |
| Persetujuan/tolak/anti-self-approval | API nyata + guard UI |
| Browser terautentikasi | Belum — `/api/v1/auth/me` lokal 401 |

Acceptance terpenuhi secara source. Transaksi submit dan persetujuan lintas akun belum dibuktikan pada browser, jadi status tetap 🟡. Perubahan sampingan: `NONE`.
