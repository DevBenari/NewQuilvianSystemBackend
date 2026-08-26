# Modul Operasi — Arsitektur Frontend

Status `approved` oleh pemilik kebutuhan pada 2026-08-21. Frontend baru boleh dikerjakan setelah kontrak backend tersedia.

## Struktur Fungsional

| Area target | Fungsi | Data utama |
|---|---|---|
| Daftar Pasien Operasi | Cari, filter, lihat status dan tindakan yang tersedia | Ringkasan `OprCase` |
| Jadwal Operasi | Kalender/daftar jadwal, ruang, tim, konflik | Schedule dan team |
| Persiapan Operasi | Consent, persiapan, checklist, sign-off | Readiness workspace |
| Pelaksanaan Operasi | Catatan tindakan, anestesi, material/implant | Execution workspace |
| Pasca Operasi | Recovery, keputusan anestesi, handover | Recovery/handover |
| Laporan | Filter dan ekspor laporan yang diizinkan | Read model laporan |

Anestesi, consent, checklist, tim, implant, diagnosis, dan dokumentasi adalah bagian kasus pasien, bukan menu terpisah.

## Aksi per Peran

| Aktor | Aksi UI yang boleh ditawarkan |
|---|---|
| Dokter pemohon/bedah | Membuat permintaan, melihat kasus terkait, mulai operasi, catatan/finalisasi/addendum, pembatalan sesuai status |
| Koordinator | Menjadwalkan, reschedule/postpone, menetapkan ruang dan tim |
| Dokter anestesi | Sign-off anestesi, catatan anestesi, keputusan recovery, pembatalan sebelum mulai |
| Perawat | Persiapan/checklist, pemakaian material, pemantauan recovery, handover |
| Penerima unit tujuan | Menerima atau menolak handover dengan alasan |
| Pengguna laporan | Melihat laporan sesuai permission |

UI hanya menampilkan action dari `availableActions` backend. Backend tetap authoritative.

## State UI

- `loading`: skeleton/indikator dan tombol command nonaktif.
- `empty`: jelaskan belum ada kasus sesuai filter.
- `error`: pesan aman, tombol retry, correlation ID bila tersedia.
- `stale`: tampilkan peringatan jika version berubah; muat ulang sebelum command.
- duplicate submit: tombol dikunci saat request berjalan dan memakai idempotency key.
- conflict `409`: tampilkan benturan ruang/tim atau data telah berubah.
- integration pending: tampilkan “menunggu sinkronisasi”, bukan gagal klinis otomatis.

## Validation dan Privasi

Validasi frontend membantu pengguna, tetapi tidak menggantikan backend. Diagnosis, catatan klinis, anestesi, komplikasi, recovery, dan handover adalah data sensitif: jangan simpan di local storage, analytics payload, URL, atau console log.

## Cache dan Invalidation

List/jadwal di-invalidasi setelah create, schedule, postpone, cancel, readiness, start, handover, dan complete. Detail kasus memakai version dari backend. Polling/realtime adalah `DEV_DISCRETION` setelah mempertimbangkan pola project; fallback manual refresh wajib tersedia.

## Accessibility dan Responsive

Status tidak boleh dibedakan hanya dengan warna. Form klinis harus dapat dipakai keyboard, label terbaca screen reader, error menunjuk field, dan layar tablet tetap mendukung pekerjaan ruang operasi. Layout, tab/drawer/modal, warna, icon, serta urutan komponen adalah `DEV_DISCRETION` mengikuti design system existing.

## Dependency

Seluruh pekerjaan frontend `BLOCKED BY` endpoint backend terkait. File `dataOperasi.jsx` dan `status-operasi.jsx` existing hanya placeholder dan bukan kontrak target.
