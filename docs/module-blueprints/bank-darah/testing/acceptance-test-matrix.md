# Bank Darah — Acceptance Test Matrix

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v1` — `draft` |
| Sumber | `00-interview-decisions.md` revisi 4 (`AC-BD-001`..`058`) · `contracts/state-transition-matrix.md` · `contracts/validation-matrix.md` |

Wajib memuat **jalur gagal**, bukan hanya jalur berhasil. Jenis test: `Unit` (aturan service), `Integ`
(service + DB), `Concurrency` (perebutan data), `E2E/UAT` (jalur pengguna). Data samaran.

---

## 1. Order darah & deteksi ganda

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-BD-001` | Ada order PRC aktif, dibuat order PRC lagi (pasien+kunjungan sama) | Integ | Ditahan, minta alasan (`VAL-BD-001`) |
| `AC-BD-002` | Ada order PRC aktif, dibuat order trombosit | Integ | Boleh dibuat |
| `AC-BD-003` | Kunjungan berbeda, dibuat order PRC | Integ | Boleh dibuat |
| `AC-BD-004` | Kunjungan RJ `Completed`, order PRC belum terpenuhi | Integ | Order berhenti menahan order baru |
| `AC-BD-010` | Order manual tanpa pasien/kunjungan/dokter/unit | Unit | Ditolak (`VAL-BD-010`) |
| `AC-BD-011` | Order tersimpan | Integ | Menyimpan pelaku input |
| `AC-BD-013` | Unit tak dikonfigurasi berwenang membuat order | Integ | Ditolak (`VAL-BD-013`) |
| `AC-BD-015` | Unit diberi kewenangan lewat konfigurasi, tanpa ubah kode | Integ | Unit langsung dapat membuat order |
| `AC-BD-016` | Unit tanpa konfigurasi kewenangan | Integ | Ditolak — tak ada kewenangan bawaan |
| `AC-BD-017` | Rawat inap `PhysicallyLeftAt` Senin siang, episode ditutup Rabu | Integ | Order tak aktif sejak Senin siang, bukan Rabu |

## 2. Permintaan PMI & penerimaan

| Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- |
| `AC-BD-005` | Minta 3 PRC, diterima 2 hari pertama | Integ | `PartiallyFulfilled`, sisa 1 |
| `AC-BD-006` | Permintaan belum dikirim, dibuat permintaan baru kebutuhan sama | Integ | Ditolak (`VAL-BD-006`) |
| `AC-BD-009` | Permintaan dikirim, darah belum diterima fisik | Integ | Stok tak bertambah |
| `AC-BD-014` | Permintaan tanpa jumlah kantong | Unit | Ditolak (`VAL-BD-007`) |
| `AC-BD-022` | Sisa 1 kantong saat kunjungan berakhir | Integ | `ClosedEncounter`, riwayat utuh |
| `AC-BD-023` | Kantong datang setelah `ClosedEncounter` | Integ | Penerimaan dicatat, kantong → `PendingReview` |
| `AC-BD-031` | Minta 2 PRC, datang 3 | Integ | `Fulfilled` sisa 0 (bukan −1); 3 kantong tercatat |
| `AC-BD-032` | Kantong ke-3 pada `AC-BD-031` | Integ | `PendingReview` + alasan "kiriman melebihi permintaan", muncul di daftar #2 |
| `AC-BD-033` | Kantong berlebih dialokasikan langsung ke order pasien sama | Integ | Ditolak (`VAL-BD-033`) |

## 3. Alokasi, bukti, pemberian, koreksi

| Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- |
| `AC-BD-018` | Kantong dialokasikan, bukti belum tercatat, tekan pemberian | Integ | Ditolak (`VAL-BD-018`) |
| `AC-BD-019` | Bukti lengkap lalu pemberian | Integ | Berhasil; kantong `Issued` |
| `AC-BD-020` | Jalur darurat oleh peran berwenang + alasan | Integ | Berhasil, ditandai tanpa bukti, muncul di daftar #3 |
| `AC-BD-021` | Jalur darurat oleh peran tak berwenang | Integ | Ditolak (`VAL-BD-021`) |
| `AC-BD-038` | Bukti tercatat, masa berlaku lewat, tekan pemberian | Integ | Ditolak (`VAL-BD-020`); bukti lama tetap riwayat |
| `AC-BD-039` | Bukti masih di dalam masa berlaku | Integ | Pemberian berhasil |
| `AC-BD-040` | Masa berlaku komponen belum dikonfigurasi | Integ | Pemberian ditahan (`VAL-BD-020b`), tak pakai nilai tebakan |
| `AC-BD-041` | Kantong berbukti pasien A dialihkan ke B, pemberian ke B | Integ | Ditolak (`VAL-BD-019`); bukti A tetap riwayat |
| `AC-BD-042` | Setelah pengalihan, bukti baru untuk B tercatat | Integ | Pemberian ke B berhasil |
| `AC-BD-043` | Batalkan alokasi, kantong belum diberikan, order aktif | Integ | Kantong kembali `Available`; riwayat tersimpan |
| `AC-BD-044` | Batalkan alokasi, order asal sudah berakhir | Integ | Kantong `PendingReview`, bukan `Available` |
| `AC-BD-045` | Batalkan alokasi tanpa alasan terkendali | Unit | Ditolak (`VAL-BD-016`) |
| `AC-BD-046` | Batalkan alokasi kantong yang sudah `Issued` | Integ | Ditolak (`VAL-BD-023`) |
| `AC-BD-047` | Koreksi oleh peran berwenang + alasan | Integ | Berhasil; pemberian asal terbaca; pemenuhan dihitung ulang |
| `AC-BD-048` | Pemberian dicoba dihapus/dianulir | Integ | Ditolak (`VAL-BD-025`) |
| `AC-BD-049` | Koreksi dipakai memindah pemberian ke pasien lain | Integ | Ditolak (`VAL-BD-049`) |
| `AC-BD-050` | Koreksi oleh peran tak berwenang | Integ | Ditolak (`VAL-BD-024`) |
| — konkurensi | Dua petugas alokasikan kantong sama untuk dua pasien | Concurrency | Tepat satu berhasil, satu `409` (`VAL-BD-018c`) |

## 4. Golongan darah & konflik

| Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- |
| `AC-BD-012` | Golongan darah pada permintaan dipakai menilai cocok | Integ | Ditolak (`VAL-BD-012`) |
| `AC-BD-028` | `MstPatient.BloodType` dipakai untuk keperluan klinis | Integ | Ditolak — bukan sumber sah (`VAL-BD-012`) |
| `AC-BD-030` | Hasil golongan darah tanpa pemeriksa/waktu | Unit | Ditolak (`VAL-BD-030`) |
| `AC-BD-034` | Hasil sah O+, muncul hasil tervalidasi baru A+ | Integ | Pasien tak punya hasil sah; gerbang tertahan; kedua hasil tersimpan |
| `AC-BD-035` | Hasil tervalidasi baru sama dengan hasil sah sebelumnya | Integ | Hasil terbaru berlaku tanpa penahanan |
| `AC-BD-036` | Konflik diselesaikan validator lewat pemeriksaan ulang | Integ | Tepat satu hasil sah; pelaku/alasan/waktu tersimpan |
| `AC-BD-037` | Konflik dicoba diselesaikan bukan validator | Integ | Ditolak (`VAL-BD-037`) |
| `AC-BD-051` | Konflik dicoba ditutup tanpa pemeriksaan ulang | Integ | Ditolak (`VAL-BD-051`) |
| `AC-BD-053` | Pemeriksaan ulang beri nilai ketiga, validator menyatakannya berlaku | Integ | Diterima — tak dipaksa cocok hasil lama |
| `AC-BD-054` | Konflik ditutup dengan sistem pilih mayoritas otomatis | Unit | Ditolak (`VAL-BD-054`) |

## 5. Kantong menunggu keputusan & tindakan

| Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- |
| `AC-BD-007` | Kunjungan berakhir, 2 kantong sudah diterima | Integ | Kedua kantong `PendingReview`, tak dapat dialokasikan ke pasien lain langsung |
| `AC-BD-008` | Ada kantong `PendingReview` | E2E | Muncul di daftar kerja #2 |
| `AC-BD-024` | Kantong `PendingReview` dialihkan dengan alasan | Integ | Berhasil; rantai pasien asal→alasan→tujuan tersimpan |
| `AC-BD-025` | Kantong `PendingReview` diselesaikan tanpa alasan | Unit | Ditolak (`VAL-BD-016`) |
| `AC-BD-029` | Alasan tidak layak diketik bebas | Unit | Ditolak (`VAL-BD-016`) |
| `AC-BD-026` | Satu tindakan selesai dengan 2 kantong diberikan | Integ | Satu fakta biaya (bila kontrak Billing turun), bukan dua — **ditandai tertunda `DEC-BD-016`** |
| `AC-BD-027` | Fakta biaya tindakan sama dikirim ulang | Integ | **Tertunda `DEC-BD-016`** — tidak diuji sampai kontrak Billing disetujui |

---

## Definition of Done (ringkas — lengkap di `04-prd-to-mvp.md`)

| Butir | Bukti |
| --- | --- |
| Satu kasus darah berjalan order → permintaan → penerimaan → periksa golongan → alokasi → bukti → pemberian | `AC-BD-005/019` + UAT jalur utama |
| Satu kantong tak mungkin diberikan ke dua pasien | `AC-BD-018c` konkurensi |
| Darah tak dapat diberikan tanpa bukti berlaku / golongan darah konflik tertahan | `AC-BD-018/038/041/034` |
| Pemberian tak dapat dihapus | `AC-BD-048` |
| Seluruh master MVP terisi | Rencana data master awal `02-backend-architecture.md` §J |
| Billing charge & label **tidak** diuji pada MVP | `DEC-BD-016`, `OQ-BD-011` — di luar cakupan |
