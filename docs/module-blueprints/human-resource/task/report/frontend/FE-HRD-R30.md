# Laporan Perubahan Frontend — `FE-HRD-R30`

## Metadata

| Field | Nilai |
| --- | --- |
| Task | `FE-HRD-R30` — Kredensial bagian 2: komite, direktur, registri |
| Task asal | `FE-HRD-191`–`195`, `198`, `199` |
| Slice | `S-C1`, Gelombang 3 |
| Kontrak | API Contract v18 bagian 24 dan source backend branch `MHamzah` |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Branch frontend | `HamzahV2` (`origin/HamzahV2`) |
| Tanggal | 13 November 2026 |
| Status | 🟡 Sebagian — dua alur memakai API nyata; enam layar berhenti aman karena kontrak daftar/detail belum tersedia |

## Hasil perubahan

- Audit KMKP sekarang mengambil `GET /clinical-privileges/emergency-overrides`, menyaring kasus pending, menampilkan DTO aktual, dan menyimpan putusan melalui `POST /emergency-overrides/review` dengan enum `Justified`, `Unjustified`, atau `DisciplinaryAction`.
- Penugasan cepat sekarang mengirim `GrantTemporaryPrivilegeDto` aktual, memakai pemilih tenaga klinis dan katalog kewenangan, membatasi provisional maksimal 90 hari serta emergency maksimal 3 hari, lalu menampilkan respons server.
- Data contoh, timeout sukses, `alert`, native table/modal, dan inline style di delapan view cakupan R30 telah dihapus.
- Menu kredensial diberi `requiredPermission` sesuai resource/action controller backend.
- Layar yang kontraknya belum cukup menampilkan status tidak tersedia secara jujur dan tidak membentuk data atau keberhasilan palsu.

## Keputusan UI

`UI GATE: REUSE/COMPOSE`. Implementasi memakai `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, keluarga `BaseFormControl`, `InformationAlert`, `ToastStack`, `HrPanel`, `HrInfoGrid`, dan `HrResourceField`. Tidak ada base component baru.

## Gap kontrak backend

| Cakupan | Gap |
| --- | --- |
| Peer review | Tidak ada worklist reviewer; `CredentialingApplicationDto` tidak membawa butir kewenangan yang dinilai |
| Pleno komite | Tidak ada worklist pleno atau matriks hasil reviewer |
| Pengesahan Direktur | Tidak ada worklist SPK-RKK yang menunggu tanda tangan |
| Verifikasi publik | Tidak ada endpoint anonim berdasarkan `VerificationHash` |
| Registri kewenangan | Endpoint hanya tersedia di bawah satu `workforceProfileId`, bukan pencarian global |
| Radar rekredensialing | Model dan worker tersedia, tetapi tidak ada endpoint daftar siklus + OPPE |
| Penugasan cepat | Endpoint create tersedia, tetapi endpoint daftar penugasan aktif tidak ada |

## Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint target R30, service, dan menu | PASS — 0 error/warning |
| CSS usage checker | PASS — 8 view, 0 temuan |
| `npm run build` | PASS — 813 halaman dan standalone runtime siap |
| Uji browser/peran | `VISUAL CHECK: NOT FEASIBLE` — tool browser tidak tersedia |

## Status penerimaan

`FE-HRD-198` terpenuhi pada source untuk worklist dan putusan KMKP. Bagian fast-track `FE-HRD-199` terpenuhi untuk penerbitan, tetapi daftar aktif dan radar rekredensialing belum dapat dipenuhi. `FE-HRD-191`–`195` terblokir gap kontrak di atas. Anti self-assessment tidak dikarang di frontend karena worklist serta identitas reviewer/pemohon belum tersedia bersama dalam respons.

Perubahan belum di-commit atau di-push.
