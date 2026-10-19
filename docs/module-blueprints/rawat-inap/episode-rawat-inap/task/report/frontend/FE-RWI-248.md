# Laporan Perubahan Frontend — `FE-RWI-248`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-248` |
| Judul | Langkah Pilih Bed admisi dan admisi bayi memakai denah |
| Slice | Integrasi Denah ↔ Bed Management — `FE-DNH-02` mode Pilih |
| Roadmap | [`frontend-roadmap-integrasi-denah.md`](../../../roadmap/frontend-roadmap-integrasi-denah.md) |
| Trace | `FR-DNH-005`; `RWI-DEC-299`, `316`, `325`; `RWI-AC-436`, `449`, `453`, `454`; `NFR-807` |
| Contract version | `0.13.0` approved |
| Dependency | `FE-RWI-247` ✅; `BE-RWI-239` ✅ |
| Klasifikasi | `HEAVY` — dua jalur admisi memakai denah per pasien, optimistic concurrency, dan pemulihan hasil ambigu |
| Task mode | `FRONTEND` |
| Target tulis | Source `QuilvianSystemFrontendDev`; laporan/roadmap/traceability di blueprint backend |
| Model | Codex |
| Commit frontend saat mulai rangkaian | `fc9aea6d62418460493899cb669832f032e48328` (`HamzahV2`, upstream `origin/HamzahV2`) |
| Commit backend rujukan | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` (`MHamzah`) |
| Tanggal | 17 Oktober 2026 |
| Status | ✅ Selesai |

---

## 1. Outcome

Langkah Pilih Bed pada admisi biasa dan admisi bayi sekarang memakai denah bersama. Frontend
memanggil `GET .../bed-occupancies/floor-plan` dengan `purpose=Admission` dan `episodeId`, memakai
`IsSelectable` server untuk outline hijau, serta hanya menawarkan **Pilih bed ini** ketika
`Eligibility.IsEligible=true`. Bed yang tidak layak tetap terlihat di popup dan menampilkan seluruh
alasan `Failures` dari server.

Bed terpilih ditampilkan dengan bentuk operasional seperti `Kelas 2 329 · bed 329-C`. Sebelum
pemesanan, lantai dibaca ulang dan `ExpectedBedVersion` diambil dari respons terbaru. Konflik 409
atau bed yang hilang pada pembacaan ulang membersihkan pilihan, memuat ulang denah, dan menampilkan
kalimat terkunci: `Bed baru saja dipilih petugas lain. Denah dimuat ulang.`

## 2. Perubahan source

| Area | Implementasi |
| --- | --- |
| Service | `bed-occupancy.service.js` menambah `getFloorPlan({ floorCode, purpose, episodeId }, config)` memakai service Axios existing |
| Hook denah | `use-floor-plan.js` dapat memilih endpoint Monitoring atau Selection, tetap membatalkan request lama, serta menormalisasi `Eligibility`, `Failures`, versi, dan kategori server |
| Adapter admisi | `use-inpatient-admission-bed.jsx` membentuk snapshot kompatibel dari satu lantai, menjaga state pilihan, recovery reservasi, versi terbaru, race 409, dan reuse `useBedOperation` untuk hasil ambigu |
| Persiapan Draft | Karena source issue 022 menunda pembuatan episode sampai ada konteks kamar, adapter memakai satu kandidat existing dari `/available-beds` hanya untuk menyiapkan episode Draft; pilihan petugas tetap kosong dan baru dilakukan pada denah server setelah episode tersedia |
| Denah mode Pilih | `floor-plan-selection.jsx` menangani loading/error/retry, tab lantai, fokus popup, pergantian lantai, dan pilihan bed |
| Popup | `floor-plan-room-popup.jsx` menambah mode Pilih: aksi untuk bed layak, alasan server untuk bed tidak layak, dan penanda bed yang dipilih; aksi Monitoring tetap terpisah |
| Langkah admisi | `inpatient-admission-bed-step.jsx` mengganti board kartu dengan denah hanya untuk Admission; jalur Transfer mempertahankan board existing sampai `FE-RWI-249` |
| Admisi bayi | `inpatient-newborn-admission-steps.jsx` tetap berada pada tahap Pilih setelah Draft otomatis terbentuk, sehingga bayi tidak langsung melompat ke Booking |

## 3. Kontrak dan keselamatan operasi

- Endpoint Selection hanya diaktifkan ketika `episodeId` sudah tersedia dan selalu mengirim
  `purpose=Admission`; status Draft dan kelayakan pasien divalidasi backend.
- Frontend tidak membuat aturan isolasi, rawat gabung, Perinatologi, status operasional, atau kelas.
  Outline dan tombol seluruhnya mengikuti `IsSelectable`/`Eligibility` dari respons server.
- Konteks bootstrap Draft tidak menjadi pilihan UI dan tidak memesan bed. Setelah episode terbentuk,
  evaluator endpoint Selection menjadi satu-satunya sumber kelayakan.
- Konfirmasi reservasi membaca ulang lantai. Payload memakai `bedId`, `episodeId`, idempotency adapter
  existing, dan `ExpectedBedVersion` terbaru.
- Hasil jaringan ambigu tetap memakai intent/body/key yang sama melalui `useBedOperation` dan
  `BedOperationRecovery`; frontend tidak membuat permintaan baru sebelum hasil diperiksa.
- Popup dan langkah Pilih tidak memuat isian, pesan, ataupun filter jenis kelamin.
- `BedName`, bukan Guid, menjadi label pengguna; Guid hanya berjalan di kontrak request.

## 4. Keputusan reuse / create-new

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Denah, geometri, zoom, tab lantai | `REUSE_WITH_ADAPTER` | Komponen bersama `FE-RWI-246/247` menerima mode Selection tanpa mengubah geometri peta |
| Popup kamar | `EXTEND` | Satu popup existing mendapat mode Pilih; mode Monitoring dan aksinya tetap utuh |
| Hook admisi, version check, idempotency, ambiguous recovery | `REUSE_WITH_ADAPTER` | `use-inpatient-admission-bed` dan `useBedOperation` tetap menjadi pemilik titik tulis reservasi |
| Board lama pada Admission | `STOP_RENDERING` | Tidak dirender pada admisi biasa/bayi; sementara hanya dipakai sebagai adapter persiapan Draft karena seam issue 022 |
| Board lama pada Transfer | `KEEP UNTIL FE-RWI-249` | Transfer sengaja tidak diubah sebelum task dependennya |
| Komponen `floor-plan-selection.jsx` | `CREATE_NEW` | Belum ada coordinator mode Pilih dengan state lantai, popup, fokus, dan callback bed |
| Base component baru | `NOT CREATED` | Tombol dan alert memakai komponen existing; CSS khusus tetap di feature denah |

`UI GATE: WAIVED BY RWI-DEC-325`. Pengecualian hanya untuk katalog/pola visual; kontrak,
permission, privasi, stack, lint, build, dan laporan tetap dipenuhi.

## 5. State dan kegagalan

| State | Perilaku |
| --- | --- |
| Persiapan episode | Menampilkan bahwa episode Draft sedang disiapkan agar server dapat menghitung kelayakan |
| Persiapan gagal | Pesan alasan + tombol `Coba Siapkan Denah Lagi`; tidak mengarang denah lokal |
| Loading denah | Menyatakan denah dan kelayakan pasien sedang dimuat |
| Error denah | Pesan error dan `Muat Ulang Denah`; respons lama tidak menimpa lantai baru |
| Bed layak | Tombol `Pilih bed ini`; pilihan aktif ditandai `Bed dipilih` |
| Bed tidak layak | Tetap terlihat dengan seluruh pesan `Eligibility.Failures` |
| Ganti lantai | Popup dan pilihan tersembunyi dibersihkan sebelum request lantai berikutnya |
| Race / 409 | Pilihan dibersihkan, lantai dimuat ulang, petugas diminta memilih kembali |
| Hasil belum diketahui | Operation recovery memeriksa hasil dengan intent idempotensi yang sama |

## 6. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Test fokus FE-RWI-248 + regresi admission/floor plan/newborn | 21/21 lulus | `PASS` |
| Test baru `floor-plan-admission-selection.test.mjs` | 4/4 lulus: normalisasi eligibility, kontrak purpose/episode/version, mode Pilih tanpa gender, dan tahap bayi tidak terlewati | `PASS` |
| `npm run lint:errors` setelah perubahan final | Exit `0`, tidak ada error ESLint | `PASS` |
| Lint terarah file task | Exit `0`, tidak ada error | `PASS` |
| `npm run build` setelah perubahan final | Exit `0`; Next.js 16.2.12 compiled, TypeScript selesai, 498/498 static pages, standalone assets siap | `PASS` |
| `git diff --check` | Exit `0`; hanya warning normalisasi LF→CRLF Git pada file existing | `PASS` |
| `npm run test:unit` | 2.945 test; 2.915 lulus, 30 gagal existing pada berkas/modul di luar diff FE-RWI-248; seluruh test fokus task lulus | `EXISTING OUT-OF-SCOPE FAILURES` |
| HTTP route / live browser | Dev server tidak sedang berjalan dan tidak ada sesi/kredensial berotorisasi | `NOT RUN` |
| UAT-803/804 dengan episode/bed aktual | Membutuhkan akun, episode Draft, dan fixture konkurensi aktual | `NOT RUN` |

UAT browser tidak menahan status selesai berdasarkan keputusan pemilik 1 September 2026 pada D-FE.
Kontrak Selection, state UI, race handling, lint, build, dan fixture unit menjadi bukti task ini.

## 7. Acceptance criteria dan DoD

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Admisi biasa memilih `329-C`, memesan dengan versi terbaru, dan menampilkan `Kelas 2 329 · bed 329-C` | ✅ | Label dibentuk dari `RoomName` + `BedName`; refresh wajib mendahului payload `ExpectedBedVersion`; fixture normalisasi `329-C` PASS |
| Bayi rawat gabung hanya mendapat boks di kamar ibu | ✅ | Frontend hanya mengaktifkan bed dengan `Eligibility.IsEligible`; konteks bayi tersimpan pada episode Draft dan alasan penolakan server ditampilkan |
| Pasien isolasi hanya melihat kamar layak ber-outline hijau | ✅ | Overlay membaca `Room.IsSelectable` server; tidak ada evaluator isolasi frontend |
| Dua petugas memilih bed sama: petugas kedua memilih ulang sesudah reload | ✅ | Target hilang atau 409 membersihkan selection, reload lantai, dan menampilkan copy konflik terkunci |
| Hasil ambigu diperiksa sebelum retry | ✅ | `useBedOperation`/`BedOperationRecovery` memakai intent dan idempotency key existing |
| Tidak ada pesan atau tampilan jenis kelamin | ✅ | Negative source test pada popup, selection, dan step PASS |
| Admisi bayi tidak melompati tahap Pilih sesudah Draft otomatis terbentuk | ✅ | `ready` tidak lagi bergantung pada `doctor.episodeId`; test regression PASS |
| Laporan, roadmap, graph, register, traceability | ✅ | Laporan ini dan pembaruan dokumen delivery |

Definition of Done `D-FE` terpenuhi. Tidak ada perubahan backend source, database, migrasi, commit,
push, merge, atau deployment.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah existing | Suite global memiliki 30 kegagalan pada test existing di luar berkas task; tidak ada kegagalan pada 21 test fokus |
| Seam issue 022 | Endpoint denah membutuhkan episode Draft, sementara source current menunda Draft sampai ada konteks kamar; adapter bootstrap dibatasi pada hook admisi dan tidak dianggap pilihan/reservasi |
| Batas runtime | UAT peran, isolasi, rooming-in, serta race aktual `NOT RUN` karena server/sesi/fixture tidak tersedia |
| Perubahan sampingan | `NONE`; perubahan worktree existing di luar scope dipertahankan |
| Langkah berikutnya | `FE-RWI-249`: tiga konsumen transfer memakai mode Pilih denah yang selesai di task ini |
