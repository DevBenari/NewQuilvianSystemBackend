# Traceability Requirement — Accounting Phase 2 (gelombang mandiri)

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 11
roadmap_revision: 1
decision_revision: 1.9
contracts: [ACC-API-0.7, ACC-STATE-0.2, ACC-VALIDATION-0.5, ACC-PERMISSION-0.4]
scope_waves: [P2-3, P2-4, P2-5]
generated_at: 2026-09-08
task_selesai: 0                       # belum ada task yang dikerjakan
```

Dokumen ini memetakan **requirement → task → bukti**. Ia dibuat sekarang, sebelum satu task pun
dikerjakan, justru supaya tidak bernasib seperti `roadmap/requirement-traceability.md` milik MVP
yang membeku pra-implementasi dan tercatat sebagai `ACC-GAP-001`.

**Aturan pemakaian:** setiap kali sebuah task selesai, baris yang bersangkutan diperbarui beserta
bukti nyatanya, dan `task_selesai` pada metadata dinaikkan. Baris yang tidak diperbarui berarti
task-nya belum benar-benar selesai, walau kodenya sudah ada.

---

## 1. Jurnal berulang (`P2-3`)

| Requirement | Isi ringkas | Keputusan asal | Task backend | Task frontend | UAT | Keadaan |
|---|---|---|---|---|---|---|
| `FR-P2-018` | Template tidak seimbang ditolak | `ACC-DEC-050` | `BE-ACC-P2-002`, `007` | `FE-ACC-P2-004` | `UAT-P2-13` | `Planned` |
| `FR-P2-019` | Template aktif menerbitkan jurnal `Draft` pada tanggalnya | `ACC-DEC-050` | `BE-ACC-P2-008` | `FE-ACC-P2-003` | `UAT-P2-11` | `Planned` |
| `FR-P2-020` | Satu template satu jurnal per periode, ditegakkan **di database** | `ACC-DEC-050` | `BE-ACC-P2-002`, `008` | — | `UAT-P2-12` | `Planned` |
| `FR-P2-021` | Tidak menerbitkan ke periode yang tidak menerima pencatatan | `ACC-DEC-050` | `BE-ACC-P2-008` | — | — | `Planned` |
| `FR-P2-022` | Template baru tidak aktif sampai diaktifkan | `ACC-DEC-050` | `BE-ACC-P2-002`, `007` | `FE-ACC-P2-003` | — | `Planned` |

## 2. Tutup bulan (`P2-4`)

| Requirement | Isi ringkas | Keputusan asal | Task backend | Task frontend | UAT | Keadaan |
|---|---|---|---|---|---|---|
| `FR-P2-023` | Daftar periksa dihitung saat diminta, bukan disimpan | `ACC-DEC-051` | `BE-ACC-P2-005` | `FE-ACC-P2-001` | — | `Planned` |
| `FR-P2-024` | Pengajuan ditolak selama ada jurnal belum sah atau kejadian gagal | `ACC-DEC-051` | `BE-ACC-P2-005`, `006` | `FE-ACC-P2-002` | `UAT-P2-14` | `Planned` |
| `FR-P2-025` | Kejadian tertahan hanya peringatan, tidak menahan | `ACC-DEC-051` | `BE-ACC-P2-005` | `FE-ACC-P2-001` | — | `Planned` |
| `FR-P2-026` | Penutupan hanya disetujui `Accounting Director` | `ACC-DEC-055` | `BE-ACC-P2-006` | `FE-ACC-P2-002` | `UAT-P2-17` | `Planned` |
| `FR-P2-027` | Penyetuju **bukan** pengaju | `ACC-DEC-016`, `052` | `BE-ACC-P2-006` | `FE-ACC-P2-002` | `UAT-P2-16` | `Planned` |
| `FR-P2-028` | Penolakan wajib beralasan, periode kembali terbuka | `ACC-DEC-052` | `BE-ACC-P2-006` | `FE-ACC-P2-002` | `UAT-P2-18` | `Planned` |
| `FR-P2-029` | Periode yang ditutup sebelum Phase 2 tetap sah tanpa riwayat | `ACC-DEC-052` | `BE-ACC-P2-006` | — | — | `Planned` |

## 3. Tutup tahun dan pengaturan (`P2-5`, `P2-0a`)

| Requirement | Isi ringkas | Keputusan asal | Task backend | Task frontend | UAT | Keadaan |
|---|---|---|---|---|---|---|
| `FR-P2-030` | Pratinjau menampilkan saldo, **tanpa membuat apa pun** | `ACC-DEC-053` | `BE-ACC-P2-010` | `FE-ACC-P2-006` | `UAT-P2-21` | `Planned` |
| `FR-P2-031` | Penyusunan ditolak bila ada periode belum tertutup | `ACC-DEC-053` | `BE-ACC-P2-010` | `FE-ACC-P2-006` | `UAT-P2-19` | `Planned` |
| `FR-P2-032` | Penyusunan ditolak bila akun laba ditahan belum ditetapkan | `ACC-DEC-054` | `BE-ACC-P2-003`, `009`, `010` | `FE-ACC-P2-005`, `006` | `UAT-P2-20` | `Planned` |
| `FR-P2-033` | Jurnal penutup `Draft` berjenis `JT`, disahkan lewat jalur yang ada | `ACC-DEC-053` | `BE-ACC-P2-003`, `010` | `FE-ACC-P2-006` | `UAT-P2-22` | `Planned` |
| `FR-P2-034` | Jurnal penutup dapat dibalik lewat pembalikan yang sudah ada | `ACC-DEC-029` | `BE-ACC-P2-010` | — | `UAT-P2-23` | `Planned` |

---

## 4. Ringkasan cakupan

| Hal | Jumlah |
|---|---:|
| Requirement dalam lingkup roadmap ini | **17** dari 34 |
| Requirement di luar lingkup (kotak masuk kejadian) | 17 |
| Task backend | 10 |
| Task frontend | 6 |
| Skenario UAT dalam lingkup | **13** dari 23 |
| Task yang sudah selesai | **0** |

## 5. Requirement tanpa UAT — coverage gap

Empat requirement tidak punya skenario UAT tersendiri. Ini **bukan** kelalaian, tetapi tetap
dicatat supaya tidak dianggap sudah teruji:

| Requirement | Kenapa tidak punya UAT | Cara membuktikannya |
|---|---|---|
| `FR-P2-021` | Perilakunya "dilewati diam-diam", tidak terlihat pengguna | Test integrasi: periode `SoftClosed`, penjadwal dijalankan, pastikan nol jurnal terbentuk dan nol galat |
| `FR-P2-022` | Bagian dari alur `UAT-P2-11`, bukan skenario sendiri | Test unit pada nilai bawaan `IsActive` |
| `FR-P2-023` | Sifat internal, tidak punya layar tersendiri | Test integrasi: panggil dua kali dengan perubahan di antaranya, pastikan angkanya berbeda |
| `FR-P2-029` | Menyangkut data lama, tidak dapat dibuat ulang lewat layar | Test integrasi memakai periode yang sengaja dibuat tanpa riwayat persetujuan |

## 6. Gap yang menyangkut bukti, bukan requirement

| Gap | Isi | Pemilik |
|---|---|---|
| `ACC-TD-016` | Nol test backend Accounting yang ada sekarang. Dua task pada roadmap ini menyentuh `AccJournalService` milik MVP tanpa jaring regresi | Rizki |
| `ACC-TEST-0.1` | `testing/acceptance-test-matrix.md` belum punya kolom bukti yang **sudah ada**. Ketiga belas UAT roadmap ini tidak punya tempat dicatat saat dijalankan | Rizki |
| `ACC-GAP-001` | `requirement-traceability.md` milik MVP masih beku pra-implementasi. Dokumen ini sengaja dibuat terpisah agar tidak mewarisi masalah yang sama | Rizki |
