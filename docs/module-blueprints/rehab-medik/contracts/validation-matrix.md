# Rehab Medik — Validation Matrix

| Field | Nilai |
|---|---|
| Blueprint | `RHB-BP-001` r1 |
| `last_changed_in` | `v1` — status `draft` |
| Owner | Sukma Giri Pratama — belum disetujui |
| Traceability | FEAT acceptance criteria sumber; RHB-INV-01..10 |

Seluruh validasi ditegakkan **di server**. FE hanya membantu pengguna mengisi. Respons gagal
memakai `ApiResponse<object>.Fail(kode, pesan, new { Code = "RHB-VAL-xxx", Fields = [...] })`
tanpa stack trace dan tanpa isi klinis.

Kode HTTP: **400** format salah; **403** tidak berhak (butir hak akses, bukan penulis, atau
profesi tidak cocok); **404** data tidak ada **atau milik pasien lain**; **409** bentrok
status/versi/duplikasi; **422** aturan bisnis tidak terpenuhi.

| ID | Berlaku pada | Kondisi ditolak | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| V-REQ-01 | Permintaan `create`/`submit` | Pasien kosong atau tidak aktif | "Pilih pasien yang terdaftar." | 422 `RHB-VAL-001` |
| V-REQ-02 | `submit` | Sumber internal tanpa kunjungan, atau kunjungan bukan milik pasien | "Pilih kunjungan pasien yang menjadi asal permintaan." | 422 `RHB-VAL-002` |
| V-REQ-03 | `submit` | Rujukan eksternal tanpa nama fasilitas dan nama perujuk | "Lengkapi nama fasilitas dan perujuk." | 422 `RHB-VAL-003` |
| V-REQ-04 | `submit` | Indikasi kosong atau < 5 karakter | "Tuliskan indikasi atau alasan rujukan." | 422 `RHB-VAL-004` |
| V-REQ-05 | `submit` | Tidak ada disiplin diminta, atau disiplin tidak aktif | "Pilih minimal satu layanan rehab yang tersedia." | 422 `RHB-VAL-005` |
| V-REQ-06 | `submit` | `RequestedAt` di masa depan | "Waktu permintaan tidak boleh melewati waktu sekarang." | 422 `RHB-VAL-006` |
| V-REQ-07 | `submit` | Ada permintaan `Submitted`/`NeedsClarification` atau episode belum `Closed` untuk pasien yang sama dengan disiplin beririsan, dan `DuplicateOverrideReason` kosong | "Pasien sudah memiliki permintaan/episode rehab yang masih berjalan. Periksa daftar di bawah, atau isi alasan untuk tetap melanjutkan." | 409 `RHB-VAL-007` (+ daftar kandidat) |
| V-REQ-08 | `return`, `request-clarification`, `redirect`, `reject`, `cancel` | Alasan kosong | "Alasan wajib diisi." | 422 `RHB-VAL-008` |
| V-REQ-09 | `accept` | Dokter penanggung jawab kosong atau tidak aktif | "Pilih dokter penanggung jawab rehab." | 422 `RHB-VAL-009` |
| V-GEN-01 | Semua ubah | `expectedVersion` berbeda | "Data sudah diubah pengguna lain. Muat ulang sebelum menyimpan." | 409 `RHB-VAL-010` |
| V-GEN-02 | Semua aksi status | Status saat ini tidak mengizinkan | "Tindakan tidak dapat dilakukan pada status saat ini." | 409 `RHB-VAL-020` |
| V-GEN-03 | Ubah dokumen klinis | Dokumen sudah final atau terkunci (`EnsureMutableAsync`) | "Catatan sudah final. Gunakan addendum untuk koreksi." | 409 `RHB-VAL-021` |
| V-GEN-04 | Ubah/tanda tangan dokumen | Pengguna bukan penulis | "Hanya penulis catatan yang dapat mengubah atau menandatanganinya." | 403 `RHB-VAL-022` |
| V-GEN-05 | Semua dokumen | Episode `Closed` | "Episode rehab sudah ditutup." | 409 `RHB-VAL-023` |
| V-GEN-06 | Lookup dan detail | Id milik pasien lain atau tidak ada | "Data tidak ditemukan." | 404 |
| V-ASM-01 | Tanda tangan asesmen medis | Tidak ada disiplin diresepkan | "Resepkan minimal satu layanan terapi sebelum menandatangani." | 422 `RHB-VAL-030` |
| V-ASM-02 | Asesmen/program/sesi disiplin | Profesi pengguna ≠ profesi disiplin, atau disiplin belum punya profesi | "Profesi Anda tidak sesuai dengan layanan ini." | 403 `RHB-VAL-031` |
| V-ASM-03 | Asesmen disiplin `create` | Disiplin belum diresepkan di episode | "Layanan ini belum diresepkan dokter rehab untuk episode ini." | 422 `RHB-VAL-032` |
| V-ASM-04 | Asesmen medis `create` | Sudah ada asesmen medis awal final | "Asesmen medis awal episode ini sudah final. Gunakan reasesmen." | 409 `RHB-VAL-033` |
| V-ASM-05 | Tanda tangan asesmen | Riwayat, temuan, masalah/diagnosis, atau kesimpulan kosong | "Lengkapi riwayat, temuan, masalah/diagnosis, dan kesimpulan." | 422 `RHB-VAL-034` |
| V-MEA-01 | Pengukuran berinstrumen | Versi instrumen bukan `Approved` (dan lingkungan tidak mengizinkan konsep), atau jawaban tidak sah menurut definisi | "Instrumen belum disahkan atau isian tidak sesuai." | 422 `RHB-VAL-035` |
| V-MEA-02 | Reasesmen | Pembanding beda instrumen/satuan dengan baseline tanpa `InstrumentChangeNote` | "Instrumen berbeda dari baseline. Jelaskan alasannya." | 422 `RHB-VAL-036` |
| V-PRG-01 | `activate` | Asesmen medis episode belum final | "Asesmen medis dokter rehab belum final." | 422 `RHB-VAL-040` |
| V-PRG-02 | `activate` | Asesmen disiplin belum final | "Asesmen profesi untuk layanan ini belum final." | 422 `RHB-VAL-041` |
| V-PRG-03 | `activate` | Tanpa intervensi, atau tindakan tidak terdaftar untuk disiplin | "Tambahkan intervensi dari daftar tindakan layanan ini." | 422 `RHB-VAL-042` |
| V-PRG-04 | `activate` | Tanpa tujuan aktif | "Kaitkan minimal satu tujuan aktif." | 422 `RHB-VAL-043` |
| V-PRG-05 | `activate` | Frekuensi, tanggal mulai, atau tanggal evaluasi kosong; tanggal evaluasi < mulai | "Lengkapi frekuensi, tanggal mulai, dan tanggal evaluasi." | 422 `RHB-VAL-044` |
| V-PRG-06 | `activate` | Edukasi terakhir untuk program ini berstatus `Refused` | "Pasien/keluarga menolak rencana ini. Program tidak dapat diaktifkan." | 422 `RHB-VAL-045` |
| V-GOL-01 | Tujuan `update` | Tujuan dirujuk program aktif | "Tujuan sedang dipakai program aktif. Gunakan revisi." | 409 `RHB-VAL-046` |
| V-GOL-02 | Tujuan `create` | Deskripsi/baseline/target kosong, atau tanggal target lampau | "Lengkapi tujuan, baseline, target, dan tanggal target." | 422 `RHB-VAL-047` |
| V-SES-01 | `start` | Program tidak `Active` | "Program terapi tidak aktif." | 409 `RHB-VAL-050` |
| V-SES-02 | `start` | Kunjungan tidak ada, bukan milik pasien, atau sudah berakhir | "Pilih kunjungan pasien hari ini yang masih terbuka." | 422 `RHB-VAL-051` |
| V-SES-03 | `start` | Pasien rawat inap sudah pulang secara fisik | "Pasien sudah pulang dari rawat inap." | 422 `RHB-VAL-052` |
| V-SES-04 | `start` | Identitas atau program belum diverifikasi | "Verifikasi identitas pasien dan program sebelum memulai." | 422 `RHB-VAL-053` |
| V-SES-05 | `start` | Kontraindikasi ditemukan tanpa keterangan | "Jelaskan kontraindikasi yang ditemukan." | 422 `RHB-VAL-054` |
| V-SES-06 | `complete` | Tanpa intervensi aktual | "Catat tindakan yang benar-benar diberikan." | 422 `RHB-VAL-055` |
| V-SES-07 | `stop-early` / deviasi | Alasan, atau tindakan segera (untuk eskalasi/henti), kosong | "Lengkapi alasan dan tindakan segera." | 422 `RHB-VAL-056` |
| V-SES-08 | Intervensi di luar rencana | `ProgramInterventionId` kosong tanpa `DeviationType = Modified` | "Tindakan di luar program wajib dicatat sebagai modifikasi beserta alasannya." | 422 `RHB-VAL-057` |
| V-SES-09 | `not-done` | Alasan kosong | "Pilih alasan sesi tidak terlaksana." | 422 `RHB-VAL-058` |
| V-SES-10 | `sign` | Ada intervensi tetapi respons kosong | "Catat respons pasien terhadap terapi." | 422 `RHB-VAL-059` |
| V-DIS-01 | Ringkasan `sign` | Alasan penutupan, layanan utama, capaian, atau kondisi akhir kosong; tindak lanjut ≠ `None` tanpa instruksi; rujukan tanpa tujuan | "Lengkapi ringkasan dan tindak lanjut." | 422 `RHB-VAL-061` |
| V-DIS-02 | Ringkasan `sign` | Masih ada dokumen episode `Draft` atau sesi `InProgress` | "Masih ada catatan yang belum final. Selesaikan atau batalkan dulu." | 409 `RHB-VAL-060` (+ daftar) |
| V-BIL-01 | Handoff | Tindakan master tidak aktif atau tarif tidak ditemukan | Status `Failed` dengan pesan "Tindakan belum bertarif. Hubungi admin billing." | — (tercatat di sesi) |
| V-CFG-01 | Disiplin `activate` | `ProfessionId` kosong atau profesi bukan klinis | "Tetapkan profesi klinis untuk layanan ini." | 422 `RHB-VAL-070` |
| V-CFG-02 | Disiplin `create` | Kode sudah dipakai | "Kode layanan sudah dipakai." | 409 `RHB-VAL-071` |
| V-PAG-01 | Semua daftar | `pageSize` > 100 | Dibatasi ke 100 (tidak ditolak) | — |

**Contoh V-REQ-07.** Pasien RM 00123456 punya episode fisioterapi `RHB-EPS-2026-000009`
berstatus `Active`. Admisi membuat permintaan fisioterapi baru dari rawat inap. Saat `submit`,
server menjawab 409 beserta episode `RHB-EPS-2026-000009`. Admisi mengisi alasan "Pindah setting
ke rawat inap pasca-operasi" lalu submit ulang, dan permintaan diterima menjadi `Submitted`.
